using log4net;
using Shared;
using Shared.Dtos.Requests;
using WebApplication.Data.Models;
using WebApplication.SPRT;
using WebApplication.Stores;

namespace WebApplication.API;

/// <summary>
/// Service for proper synchronization of the test.
/// </summary>
public class TestService : ITestService
{
    private readonly WorkerLogStore _workerLogStore;
    private readonly TestStore _testStore;
    private readonly PentaStore _pentaStore;
    private readonly AutobenchStateStore _autobenchStateStore;
    private readonly TestBranchStore _testBranchStore;
    private readonly UserStore _userStore;
    private readonly WorkerErrorStore _workerErrorStore;
    private readonly OpeningBookStore _openingBookStore;
    
    private static readonly SemaphoreSlim _testStateSemaphore = new SemaphoreSlim(1, 1);
    private static readonly ILog _logger = LogManager.GetLogger(typeof(TestService));
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestService(WorkerLogStore workerLogStore, TestStore testStore, PentaStore pentaStore, AutobenchStateStore autobenchStateStore, TestBranchStore testBranchStore, UserStore userStore, WorkerErrorStore workerErrorStore, OpeningBookStore openingBookStore)
    {
        _workerLogStore = workerLogStore;
        _testStore = testStore;
        _pentaStore = pentaStore;
        _autobenchStateStore = autobenchStateStore;
        _testBranchStore = testBranchStore;
        _userStore = userStore;
        _workerErrorStore = workerErrorStore;
        _openingBookStore = openingBookStore;
    }
   
    /// <inheritdoc /> 
    public bool HandleRunningTestFromWorker(int connectionId)
    {
        _testStateSemaphore.Wait();
        try
        {
            var workerLog = _workerLogStore.GetByConnectionId(connectionId);
            if (workerLog is null) throw new NotFoundException();
            if (workerLog.Test.State is TestState.Stopped or TestState.Finished) return false;
            if (workerLog.State != WorkerLogState.Active) throw new NotFoundException();
        
            _testStore.SetRunningState(workerLog.Test);
            workerLog.SetLastConnectTimeNow();
                
            _workerLogStore.SaveChanges();
        
            var running = workerLog.Test.State.Running();
            return running;
        }
        catch (NotFoundException)
        {
            throw new NotFoundException();
        }
        finally
        {
            _testStateSemaphore.Release();
        }
    }
    
    /// <inheritdoc /> 
    public async Task SaveTestError(TestErrorDto testErrorDto)
    {
        await _testStateSemaphore.WaitAsync();
        try
        {
            var workerLog = _workerLogStore.GetByConnectionId(testErrorDto.ConnectionId);
            if (workerLog is null || testErrorDto.Log.Length > Constants.MAX_LOG_FILE_SIZE
                || workerLog.Error != null || workerLog.State != WorkerLogState.Active
                || !workerLog.Test.State.Running()) throw new NotFoundException();

            workerLog.State = WorkerLogState.Finished;
            _workerLogStore.AddError(workerLog, testErrorDto.Log);
            
            await StopTestPr(workerLog.Test.Id);
        }
        catch (NotFoundException)
        {
            throw new NotFoundException();
        }
        finally
        {
            _testStateSemaphore.Release();
        }
    }
    
    /// <inheritdoc /> 
    public async Task<bool> UpdateSPRTResults(ResultsDto resultsDto)
    {
        await _testStateSemaphore.WaitAsync();
        try
        {
            var workerLog = _workerLogStore.GetByConnectionId(resultsDto.ConnectionId);
 
            if (workerLog is null) throw new NotFoundException();
            if (workerLog.Test.State != TestState.Running) return false;
            if (workerLog.State != WorkerLogState.Active) throw new NotFoundException();
            
            // Test can be eventually deleted !
            var test = _testStore.GetById(workerLog.Test.Id);
            if (test is null) throw new NotFoundException();

            var toIncrement = (resultsDto.Ll + resultsDto.Ld + resultsDto.Dd + resultsDto.Wl + resultsDto.Wd + resultsDto.Ww) * 2;
            if (workerLog.NumberOfGames + toIncrement > workerLog.TotalNumberOfGames) throw new NotFoundException();

            await _pentaStore.UpdatePenta(workerLog.Test.Id, resultsDto.Ll, resultsDto.Ld, resultsDto.Dd, resultsDto.Wl,
                resultsDto.Wd, resultsDto.Ww);
            workerLog.NumberOfGames += toIncrement;
            if (workerLog.NumberOfGames == workerLog.TotalNumberOfGames)
            {
                workerLog.State = WorkerLogState.Finished;
            }

            _workerLogStore.Update(workerLog);

            var running = await _testStore.SetPausedIfNoActiveWorkers(workerLog.Test.Id);

            // SPRT part.
            var statistics = Sprt.GetStatistics(test);
            if (statistics.Result != Sprt.SprtResult.Unknown)
            {
                await _workerLogStore.StopAllWorkers(workerLog.Test.Id);
                await _testStore.SetFinishedState(test.Id);
            }

            return running && statistics.Result == Sprt.SprtResult.Unknown;
        }
        catch (Exception ex)
        {   
            LogIfNotNotFoundException(ex);
            throw new NotFoundException();
        }
        finally
        {
            _testStateSemaphore.Release();
        }
    }

    /// <inheritdoc /> 
    public async Task UpdateAutobenchState(AutobenchDto autobenchDto)
    {
        await _testStateSemaphore.WaitAsync();
        try
        {
            var workerLog = _workerLogStore.GetByConnectionId(autobenchDto.ConnectionId);
            if (workerLog is null || workerLog.State != WorkerLogState.Active) throw new NotFoundException();

            var autobenchState = _autobenchStateStore.GetAutobenchStateByTestId(workerLog.Test.Id);
            if (autobenchState is null) throw new NotFoundException();
            if (autobenchState.Resolved) return;

            var result = autobenchState.UpdateConfidence(autobenchDto.Autobench);
            if (!result)
            {
                await StopTestPr(workerLog.Test.Id);
            }

            workerLog.State = WorkerLogState.Finished;
            _workerLogStore.SaveChanges();
            _autobenchStateStore.SaveChanges();

            if (result)
            {
                await _testStore.SetPausedIfNoActiveWorkers(workerLog.Test.Id);
            }

            // If test is resolved, set this as a bench of the test branch.
            if (autobenchState.Resolved)
            {
                await _testBranchStore.SetTestBranchBench(workerLog.Test.Id, autobenchState.Bench);
            }
        }
        catch (Exception ex)
        {
            LogIfNotNotFoundException(ex);
            throw new NotFoundException();
        }
        finally
        {
            _testStateSemaphore.Release();
        }
    }
    
    /// <inheritdoc /> 
    public async Task<(WorkerLog, Test)> CreateJobForWorker(GetTestDto getTestDto, string userToken)
    {
        await _testStateSemaphore.WaitAsync();
        try
        {
            var test = _testStore.GetNextTestForWorker(getTestDto.Autobench, getTestDto.NumberOfThreads);
            if (test is null) throw new NotFoundException();

            var user = _userStore.GetUserByAccessToken(userToken);
        
            _workerLogStore.Attach(user!);
            _workerLogStore.Attach(test);

            var now = DateTime.UtcNow;
            var wl = new WorkerLog
            {
                Name = getTestDto.Name.Length <= WorkerLog.MAX_NAME_LENGTH ? getTestDto.Name : getTestDto.Name[..(WorkerLog.MAX_NAME_LENGTH - 1)],
                State = WorkerLogState.Active,
                ConnectTime = now,
                LastConnectTime = now,
                NumberOfGames = 0,
                TotalNumberOfGames = getTestDto.Autobench ? 0 : TotalNumberGamesCalculator.Calculate(getTestDto.NumberOfThreads, test.NumberOfThreads), 
                NumberOfThreads = getTestDto.NumberOfThreads,
                Mac = getTestDto.Mac,
                User = user!, // User exists - middleware validated that.
                Test = test
            };
        
            _workerLogStore.Add(wl);
            _workerLogStore.SaveChanges();
            
            // We need to set running state after WorkerLog is created if service is down,
            // there is no WorkerLog, but test is in the running state -> invalid. 
            await _testStore.SetRunningState(test.Id);
            return (wl, test);
        }
        catch (Exception ex)
        {
            LogIfNotNotFoundException(ex);
            throw new NotFoundException();
        }
        finally
        {
            _testStateSemaphore.Release();
        }
    }
    
    /// <inheritdoc /> 
    public async Task SetPausedIfNoActiveWorkers(int testId)
    {
        await _testStateSemaphore.WaitAsync();
        try
        {
            await _testStore.SetPausedIfNoActiveWorkers(testId);
        }
        catch (Exception ex)
        {
            LogIfNotNotFoundException(ex);
        }
        finally
        {
            _testStateSemaphore.Release();
        }
    }
    
    /// <inheritdoc /> 
    public string GetUsernameByAccessToken(string accessToken)
        => _userStore.GetUserByAccessToken(accessToken)!.UserName!;

    /// <inheritdoc /> 
    public void AddWorkerError(WorkerErrorDto workerErrorDto)
        => _workerErrorStore.AddError(workerErrorDto.Log);
    
    /// <inheritdoc /> 
    public int GetTotalPausedTestsWithMaxPriority()
        => _testStore.TotalPausedTestsWithMaxPriority();

    /// <inheritdoc />
    public int GetMaxThreadsForTestWithMaxPriority()
        => _testStore.MaxThreadsForTestWithMaxPriority();

    /// <inheritdoc />
    public TestBranch? GetTestBranch(int testBranchId)
        => _testBranchStore.GetById(testBranchId);

    /// <inheritdoc />
    public byte[] GetContentForOpeningBook(int openingBookId)
        => _openingBookStore.LoadContent(openingBookId);

    /// <inheritdoc />
    public async Task StopTest(int testId)
    {
        await _testStateSemaphore.WaitAsync();
        try
        {
            await StopTestPr(testId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message, ex);
        }
        finally
        {
            _testStateSemaphore.Release();
        }
    }

    private async Task StopTestPr(int testId)
    {
        await _testStore.StopTest(testId);
        await _workerLogStore.StopAllWorkers(testId);
    }
    
    private static void LogIfNotNotFoundException(Exception ex)
    {
        if (ex is not NotFoundException)
        {
            _logger.Error(ex.Message, ex);   
        }
    }
}