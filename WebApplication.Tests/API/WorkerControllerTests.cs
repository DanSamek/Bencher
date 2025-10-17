using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using WebApplication.API;
using WebApplication.API.Dtos.Requests;
using WebApplication.API.Dtos.Responses;
using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.API;

[TestFixture]
public class WorkerControllerTests : WorkerControllerTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        RefreshController();
        
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("uho")
            .CreateSprtSettings()
            .CreateUser("user_1")
                .Close()
            .CreateUser("user_2")
                .WithAccessToken("87654321")
                .AddEngine("sentinel")
                    .AddBranch("base_branch")   
                    .AddBranch("test_branch")   
                .Close()
                .Close()
            .CreateUser("user_3")
                .WithAccessToken("12345678")
                .AddEngine("stockfish")
                .AddBranch("base_branch")
                .AddBranch("test_branch")
                .AddBranch("test_branch_2")
                .AddTest("test_31", "uho", "base_branch", "test_branch")
                    .EnsurePentaCreated(Factory.CreateDbContext())
                    .Close()
                .Close()
            .Close()
        .Close();
        
        EngineBuilder.AddAutobenchedTestForUser("test_21", "uho", "base_branch", 
            "test_branch", "sentinel", "user_2", Factory.CreateDbContext());
        
        EngineBuilder.AddAutobenchedTestForUser("test_22", "uho", "base_branch", 
            "test_branch", "stockfish", "user_2", Factory.CreateDbContext()); 
        
        EngineBuilder.AddAutobenchedTestForUser("test_33", "uho", "base_branch", 
            "test_branch", "stockfish", "user_3", Factory.CreateDbContext());
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.Validate" />, when valid access token is in the headers.
    /// NOTE: <see cref="WorkerMiddleware" /> validates, if access token is in the header. 
    /// </summary>
    [TestCase("12345678", "user_3")]
    [TestCase("87654321", "user_2")]
    public void Validate_ValidToken(string accessToken, string username)
    {
        SetAccessToken(accessToken);
        var result = Controller.Validate();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var dto = GetResponseValue<ValidateResponseDto, OkObjectResult>(result);;
        Assert.That(dto, Is.Not.Null);
        Assert.That(username, Is.EqualTo(dto.Username));
    }

    /// <summary>
    /// Test for <see cref="WorkerController.Validate" />, when invalid access token is in the headers.
    /// NOTE: <see cref="WorkerMiddleware" /> validates, if access token is in the header. 
    /// </summary>
    [Test]
    public void Validate_InvalidToken()
    {
        SetAccessToken("154123");
        var result = Controller.Validate();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.GetTest" /> - autobench.
    /// </summary>
    [Test]
    public void GetTest_Autobench()
    {
        LoginAs("user_2");
        var dto = new GetTestDto
        {
            Autobench = true,
            Mac = "AA:BB:CC:DD:EE:FF",
            Name = "WORKSTATION_PC",
            NumberOfThreads = 1
        };
        
        var result = Controller.GetTest(dto);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        
        var resultDto = GetResponseValue<GetTestAutobenchResponse, OkObjectResult>(result);
        Assert.That(resultDto, Is.Not.Null);
        Assert.That(resultDto.GitUrl, Is.EqualTo("git-url-sentinel"));
        
        using var validationContext = Factory.CreateDbContext();
        Assert.That(validationContext.WorkerLogs.Count(), Is.EqualTo(1));
        var workerLog = validationContext.WorkerLogs
            .AsNoTracking()
            .Include(t => t.Test)
            .First();
        
        Assert.That(workerLog.Test.Name, Is.EqualTo("test_21"));
        
        var test = validationContext.Tests
            .AsNoTracking()
            .Include(test => test.WorkerLogs)
            .First(t => t.Name == "test_21");
        
        Assert.That(test.WorkerLogs, Has.Count.EqualTo(1));
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.GetTest" /> - normal test [no autobench].
    /// </summary>
    [Test]
    public void GetTest()
    {
        LoginAs("user_2");
        var dto = new GetTestDto
        {
            Autobench = false,
            Mac = "AA:BB:CC:DD:EE:FF",
            Name = "WORKSTATION_PC",
            NumberOfThreads = 1
        };
        
        var result = Controller.GetTest(dto);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        
        var resultDto = GetResponseValue<GetTestNonAutobenchResponse, OkObjectResult>(result);
        Assert.That(resultDto, Is.Not.Null);
        Assert.That(resultDto.GitUrl, Is.EqualTo("git-url-stockfish"));
        Assert.That(resultDto.OpeningBook.Data, Is.EqualTo(new int[] { 0x69 }));

        using var validationContext = Factory.CreateDbContext();
        Assert.That(validationContext.WorkerLogs.Count(), Is.EqualTo(1));
        var workerLog = validationContext.WorkerLogs
            .AsNoTracking()
            .Include(t => t.Test)
            .First();
        
        Assert.That(workerLog.Test.Name, Is.EqualTo("test_31"));
        
        var test = validationContext.Tests
            .AsNoTracking()
            .Include(test => test.WorkerLogs)
            .First(t => t.Name == "test_31");
        
        Assert.That(test.WorkerLogs, Has.Count.EqualTo(1));
    }

    /// <summary>
    /// Test for <see cref="WorkerController.GetTest" /> - when there is no test in the database.
    /// We expect 404.
    /// </summary>
    [Test]
    public async Task GetTest_NoTest()
    {
        await Factory.CreateDbContext().Tests.ExecuteDeleteAsync();
        
        var dto = new GetTestDto
        {
            Autobench = false,
            Mac = "AA:BB:CC:DD:EE:FF",
            Name = "WORKSTATION_PC",
            NumberOfThreads = 1
        };
        LoginAs("user_2");
        
        var result = Controller.GetTest(dto);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        
        var resultDto = GetResponseValue<ResponseBase, NotFoundObjectResult>(result);
        Assert.That(resultDto, Is.Not.Null);
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.RunningTest" /> - normal test.
    /// We expect, that test will be in the running state after method call.
    /// </summary>
    [Test]
    public void RunningTest()
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestNonAutobenchResponse>(false);
        var runningTest = GetTestByConnectionId(resultDto.ConnectionId);
        Assert.That(runningTest.State, Is.EqualTo(TestState.Paused));
        
        RefreshController();
        Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        runningTest = GetTestByConnectionId(resultDto.ConnectionId);
        Assert.That(runningTest.State, Is.EqualTo(TestState.Running));
        
        CheckDifferentConnectTimes(resultDto.ConnectionId);
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.RunningTest" /> - autobenched.
    /// We expect, that test will be in the autobenched after method call.
    /// </summary>
    [Test]
    public void RunningTest_Autobenched()
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestAutobenchResponse>(true);
        var runningTest = GetTestByConnectionId(resultDto.ConnectionId);
        Assert.That(runningTest.State, Is.EqualTo(TestState.Paused));
        
        RefreshController();
        Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        runningTest = GetTestByConnectionId(resultDto.ConnectionId);
        Assert.That(runningTest.State, Is.EqualTo(TestState.Autobenched));
        CheckDifferentConnectTimes(resultDto.ConnectionId);
    }

    /// <summary>
    /// Test for <see cref="WorkerController.RunningTest" /> - with invalid an Id.
    /// </summary>
    [Test]
    public void RunningTest_InvalidConnectionId()
    {
        var result = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = 5555
        });

        var resultDto = GetResponseValue<ResponseBase, NotFoundObjectResult>(result);
        Assert.That(resultDto,  Is.Not.Null);
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.Autobench" /> - with same autobench value as a stored one.
    /// We expect, that AutobenchState Confidence won't be 0.
    /// </summary>
    [Test]
    public async Task Autobench_SameAsUserValue()
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestAutobenchResponse>(true);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });

        var test = GetTestByConnectionId(resultDto.ConnectionId);
        var testBranchBench = TestBranchBench(test.Id);
        Assert.That(test.AutobenchState!.Confidence, Is.EqualTo(0.0));
        
        RefreshController();
        var result = await Controller.Autobench(new AutobenchDto
        {
            Autobench = testBranchBench,
            ConnectionId = resultDto.ConnectionId
        });

        var resultDto2 = GetResponseValue<ResponseBase, OkObjectResult>(result);
        Assert.That(resultDto2, Is.Not.Null);
        test = GetTestByConnectionId(resultDto.ConnectionId);
        Assert.That(test.AutobenchState!.Confidence, Is.Not.EqualTo(0.0));
        Assert.That(test.State, Is.EqualTo(TestState.Autobenched));
    }

    /// <summary>
    /// Test for <see cref="WorkerController.Autobench" /> - with a different autobench value as a stored one.
    /// We expect, that AutobenchState Confidence will be still 0 and test state is stopped.
    /// </summary>
    [Test]
    public async Task Autobench_DifferentFromUserValue()
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestAutobenchResponse>(true);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        var test = GetTestByConnectionId(resultDto.ConnectionId);
        Assert.That(test.AutobenchState!.Confidence, Is.EqualTo(0.0));
        Assert.That(test.State, Is.EqualTo(TestState.Autobenched));
        
        RefreshController();
        var result = await Controller.Autobench(new AutobenchDto
        {
            Autobench = 1,
            ConnectionId = resultDto.ConnectionId
        });

        var resultDto2 = GetResponseValue<ResponseBase, OkObjectResult>(result);
        Assert.That(resultDto2, Is.Not.Null);
        test = GetTestByConnectionId(resultDto.ConnectionId);
        Assert.That(test.AutobenchState!.Confidence, Is.EqualTo(0.0));
        Assert.That(test.State, Is.EqualTo(TestState.Stopped));
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.Autobench" /> - with invalid connectionId.
    /// We expect, controller will return 404.
    /// </summary>
    [Test]
    public async Task Autobench_InvalidConnectionId()
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestAutobenchResponse>(true);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        var test = GetTestByConnectionId(resultDto.ConnectionId);
        RefreshController();
        var result = await Controller.Autobench(new AutobenchDto
        {
            Autobench = test.AutobenchState!.Bench,
            ConnectionId = 100000
        });

        var resultDto2 = GetResponseValue<ResponseBase, NotFoundObjectResult>(result);
        Assert.That(resultDto2, Is.Not.Null);
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.Autobench" /> - with invalid connectionId - not autobeched test.
    /// We expect, controller will return 404.
    /// </summary>
    [Test]
    public async Task Autobench_ConnectionId_NotAutobench()
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestNonAutobenchResponse>(false);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        RefreshController();
        var result = await Controller.Autobench(new AutobenchDto
        {
            Autobench = 555555,
            ConnectionId = resultDto.ConnectionId
        });

        var resultDto2 = GetResponseValue<ResponseBase, NotFoundObjectResult>(result);
        Assert.That(resultDto2, Is.Not.Null);
    }

    /// <summary>
    /// Test for <see cref="WorkerController.Results" /> - test is running.
    /// We expect, that penta stats will be updated.
    /// </summary>
    [Test]
    public async Task Results() 
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestNonAutobenchResponse>(false, 4);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
                
        RefreshController();
        var result = await Controller.Results(new ResultsDto
        {
            Ll = 1,
            Ld = 2,
            Dd = 3,
            Wl = 4,
            Wd = 5,
            Ww = 6,
            ConnectionId = resultDto.ConnectionId
        });

        var responseDto = GetResponseValue<ResultsResponseDto, OkObjectResult>(result);
        Assert.That(responseDto, Is.Not.Null);
        Assert.That(responseDto.Running);
        
        var testId = GetTestByConnectionId(resultDto.ConnectionId).Id;
        var penta = Factory.CreateDbContext().Pentas.First(x => x.TestId == testId);
        Assert.Multiple(() =>
        {
            Assert.That(penta.Ll, Is.EqualTo(1));
            Assert.That(penta.Ld, Is.EqualTo(2));
            Assert.That(penta.Dd, Is.EqualTo(3));
            Assert.That(penta.Wl, Is.EqualTo(4));
            Assert.That(penta.Wd, Is.EqualTo(5));
            Assert.That(penta.Ww, Is.EqualTo(6));
        });
    }

    /// <summary>
    /// Test for <see cref="WorkerController.Results" /> -- Multithreaded -- test is running.
    /// We expect, that penta stats will be updated + no dataraces when updates.
    /// </summary>
    [Test]
    public async Task Results_Multithreaded()
    {
        var workerThreadsCounts = new[] { 12, 10, 1, 1, 1, 12, 1, 2, 4, 5, 4, 4, 4, 2};
        var bag = new ConcurrentBag<SimplePenta>();

        var tasks = new List<Task>();
        var dtos = new List<GetTestNonAutobenchResponse>();

        foreach (var workerThreadCounts in workerThreadsCounts)
        {
            RefreshController();
            LoginAs("user_3");
            var resultDto = GetTest<GetTestNonAutobenchResponse>(false, workerThreadCounts);
            dtos.Add(resultDto);
            RefreshController();
            Controller.RunningTest(new RunningTestDto
            {
                ConnectionId = resultDto.ConnectionId
            });
        }
        
        foreach (var dto in dtos)
        {
            tasks.Add(Task.Run(async () =>
            {
                await SimulateResults(dto.ConnectionId, dto.NumberOfGames);
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        var pentaSums = new SimplePenta(new int[6]);
        
        foreach (var simplePenta in bag)
        {
            pentaSums.Data[SimplePenta.LL] += simplePenta.At(SimplePenta.LL);
            pentaSums.Data[SimplePenta.LD] += simplePenta.At(SimplePenta.LD);
            pentaSums.Data[SimplePenta.DD] += simplePenta.At(SimplePenta.DD);
            pentaSums.Data[SimplePenta.WL] += simplePenta.At(SimplePenta.WL);
            pentaSums.Data[SimplePenta.WD] += simplePenta.At(SimplePenta.WD);
            pentaSums.Data[SimplePenta.WW] += simplePenta.At(SimplePenta.WW);
        }

        var test = GetTestByConnectionId(dtos[0].ConnectionId);
        var penta = Factory.CreateDbContext().Pentas.First(p => p.TestId == test.Id);
        Assert.Multiple(() =>
        {
            Assert.That(penta.Ll, Is.EqualTo(pentaSums.Data[SimplePenta.LL]));
            Assert.That(penta.Ld, Is.EqualTo(pentaSums.Data[SimplePenta.LD]));
            Assert.That(penta.Dd, Is.EqualTo(pentaSums.Data[SimplePenta.DD]));
            Assert.That(penta.Wl, Is.EqualTo(pentaSums.Data[SimplePenta.WL]));
            Assert.That(penta.Wd, Is.EqualTo(pentaSums.Data[SimplePenta.WD]));
            Assert.That(penta.Ww, Is.EqualTo(pentaSums.Data[SimplePenta.WW]));
        });
        
        return;
        async Task SimulateResults(int connectionId, int numberOfGames)
        {
            var exponent = Random.Shared.Next(1, Math.Min(numberOfGames / 8, 4));
            var iterPairs = numberOfGames / (int)Math.Pow(2, exponent);
            var numberOfPairs = numberOfGames / 2;
            while (numberOfPairs > 0)
            {
                var controller = new WorkerController(new UserStore(Factory), new WorkerLogStore(Factory),
                    new PentaStore(Factory),
                    new TestStore(Factory), new TestBranchStore(Factory), new AutobenchStateStore(Factory));

                var simplePenta = SimplePenta.Generate(iterPairs);
                bag.Add(simplePenta);
                
                var dto = new ResultsDto
                {
                    Ll = simplePenta.Data[SimplePenta.LL],
                    Ld = simplePenta.Data[SimplePenta.LD],
                    Dd = simplePenta.Data[SimplePenta.DD],
                    Wl = simplePenta.Data[SimplePenta.WL],
                    Wd = simplePenta.Data[SimplePenta.WD],
                    Ww = simplePenta.Data[SimplePenta.WW],
                    ConnectionId = connectionId
                };
                await controller.Results(dto);
                numberOfPairs -= iterPairs;
            }
        }
    }

    private record SimplePenta(int[] Data)
    {
        public static int LL = 0;
        public static int LD = 1;
        public static int WL = 2;
        public static int DD = 3;
        public static int WD = 4;
        public static int WW = 5;

        public int At(int index)
        {
            return Data[index];
        }
        
        public static SimplePenta Generate(int totalGames)
        {
            var data = new int[6];
            while (totalGames > 0)
            {
                var index = Random.Shared.Next(0, 6);
                data[index]++;
                totalGames--;
            }
            var result = new SimplePenta(data);
            return result;
        }
    }
    
    
    /// <summary>
    /// Test for <see cref="WorkerController.Results" /> - invalid connection id.
    /// </summary>
    [Test]
    public async Task Results_InvalidConnectionId() 
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestNonAutobenchResponse>(false);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        RefreshController();
        var result = await Controller.Results(new ResultsDto
        {
            Ll = 1,
            Ld = 2,
            Dd = 3,
            Wl = 4,
            Wd = 5,
            Ww = 6,
            ConnectionId = 55555
        });
        
        var responseDto = GetResponseValue<ResponseBase, NotFoundObjectResult>(result);
        Assert.That(responseDto, Is.Not.Null);
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.Results" /> - but test is not running for penta update.
    /// We expect, that penta won't be changed.
    /// </summary>
    [Test]
    public async Task Results_NotRunningTest() 
    {
        LoginAs("user_2");
        var resultDto = GetTest<GetTestNonAutobenchResponse>(false, 4);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        RefreshController();
        _ = await Controller.Results(new ResultsDto
        {
            Ll = 1,
            Ld = 2,
            Dd = 3,
            Wl = 4,
            Wd = 5,
            Ww = 6,
            ConnectionId = resultDto.ConnectionId
        });


        var test = GetTestByConnectionId(resultDto.ConnectionId);
        await Factory.CreateDbContext().Tests.Where(t => t.Id == test.Id)
            .ExecuteUpdateAsync(psc => psc.SetProperty(t => t.State, TestState.Stopped));

        RefreshController();
        var result = await Controller.Results(new ResultsDto
        {
            Ll = 1,
            Ld = 2,
            Dd = 3,
            Wl = 4,
            Wd = 5,
            Ww = 6,
            ConnectionId = resultDto.ConnectionId
        });
        
        var responseDto = GetResponseValue<ResultsResponseDto, OkObjectResult>(result);
        Assert.That(responseDto, Is.Not.Null);
        
        Assert.That(!responseDto.Running);
        var penta = Factory.CreateDbContext().Pentas.First(x => x.TestId == test.Id);
        Assert.Multiple(() =>
        {
            Assert.That(penta.Ll, Is.EqualTo(1));
            Assert.That(penta.Ld, Is.EqualTo(2));
            Assert.That(penta.Dd, Is.EqualTo(3));
            Assert.That(penta.Wl, Is.EqualTo(4));
            Assert.That(penta.Wd, Is.EqualTo(5));
            Assert.That(penta.Ww, Is.EqualTo(6));
        });
        
    }

    /// <summary>
    /// Test for <see cref="WorkerController.Error" /> - valid file upload.
    /// </summary>
    [Test]
    public void Error()
    { 
        LoginAs("user_2");
        var resultDto = GetTest<GetTestNonAutobenchResponse>(false);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        var array = new byte [] {0x1, 0x2, 0x3, 0x4};
        using var stream = new MemoryStream(array);
        var file = new FormFile(stream, 0, array.Length, "log", "log.txt");
        
        RefreshController();
        var result = Controller.Error(new ErrorDto
        {
            Log = file,
            ConnectionId = resultDto.ConnectionId
        });

        var response = GetResponseValue<ResponseBase, OkObjectResult>(result);
        Assert.That(response, Is.Not.Null);
        
        var test = GetTestByConnectionId(resultDto.ConnectionId);
        var testError = Factory.CreateDbContext()
            .Errors
            .Include(t => t.Test)
            .First(x => x.Test.Id == test.Id);
        
        Assert.That(testError.Log, Is.EqualTo(new int[] {0x1, 0x2, 0x3, 0x4}));
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.Error" /> - but ConnectionId is invalid.
    /// We expect, that will be returned 404.
    /// </summary>
    [Test]
    public void Error_InvalidConnectionId()
    { 
        LoginAs("user_2");
        var resultDto = GetTest<GetTestNonAutobenchResponse>(false);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });
        
        var array = new byte [] {0x1, 0x2, 0x3, 0x4};
        using var stream = new MemoryStream(array);
        var file = new FormFile(stream, 0, array.Length, "log", "log.txt");
        
        RefreshController();
        var result = Controller.Error(new ErrorDto
        {
            Log = file,
            ConnectionId = 5000000
        });

        var response = GetResponseValue<ResponseBase, NotFoundObjectResult>(result);
        Assert.That(response, Is.Not.Null);
    }
    
    /// <summary>
    /// Test for <see cref="WorkerController.Error" />.
    /// </summary>
    [Test]
    public void Error_Autobenched()
    { 
        LoginAs("user_2");
        var resultDto = GetTest<GetTestAutobenchResponse>(true);
        RefreshController();
        _ = Controller.RunningTest(new RunningTestDto
        {
            ConnectionId = resultDto.ConnectionId
        });

    }
    
    private int TestBranchBench(int testId)
    {
        var testBranchBench = Factory.CreateDbContext()
            .Tests
            .Include(t => t.AutobenchState)
            .First(t => t.Id == testId)
            .AutobenchState!
            .Bench;
        return testBranchBench;
    }
    
    private void CheckDifferentConnectTimes(int connectionId)
    {
        var test = Factory.CreateDbContext()
            .Tests
            .AsNoTracking()
            .Include(test => test.WorkerLogs)
            .First(t => t.WorkerLogs.Any(wl => wl.Id == connectionId));
        
        Assert.That(test.WorkerLogs, Has.Count.EqualTo(1));
        Assert.That(test.WorkerLogs[0].ConnectTime, Is.Not.EqualTo(test.WorkerLogs[0].LastConnectTime));
    }
    
    private TDto GetTest<TDto>(bool autobench, int numberOfThreads = 1)
    {
        var dto = new GetTestDto
        {
            Autobench = autobench,
            Mac = "AA:BB:CC:DD:EE:FF",
            Name = "WORKSTATION_PC",
            NumberOfThreads = numberOfThreads
        };
        var result = Controller.GetTest(dto);
        var resultDto = GetResponseValue<TDto, OkObjectResult>(result)!;
        return resultDto;
    }

    private void SetAccessToken(string accessToken)
    {
        Controller.ControllerContext.HttpContext = new DefaultHttpContext();
        Controller.HttpContext.Request.Headers.Add(new KeyValuePair<string, StringValues>(Shared.WORKER_REQUEST_HEADER, accessToken));
    }
    
    private Test GetTestByConnectionId(int id)
    {
        var test = Factory.CreateDbContext()
            .WorkerLogs
            .AsNoTracking()
            .Include(workerLog => workerLog.Test)
                .ThenInclude(t => t.AutobenchState)
            .First(wl => wl.Id == id)
            .Test;

        return test;
    }
}