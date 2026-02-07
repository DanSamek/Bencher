using Shared;
using Shared.Dtos.Responses;
using Worker.TestProcessors.GameTestProcessor;

namespace Worker.Tests.TestProcessors.GameTestProcessor;

[TestFixture]
public class FastchessArgumentsCreatorTests
{
    public record TestCase(
            string OpeningBookPath,
            string BaseDirectory,
            string NewDirectory,
            int BaseNps,
            GetTestNonAutobenchResponse Response,
            int ProcessorThreads,
            string ExpectedArguments
        );

    private static readonly TestCase[] _testCases = [
        //  -resign movecount=3 score=500 [additional fastchess options]
        
        new TestCase("/tmp/openingbook.epd", "/tmp/base", "/tmp/new", 500000, new GetTestNonAutobenchResponse
        {
            ExpectedNps = 500000,   
            HashSize = 16,
            NumberOfThreads = 2,
            TimeManagement = "5+0.50",
            NumberOfGames = 32,
            AdditionalFastchessOptions = "",
            OpeningBook = new OpeningBookDto("", [], OpeningBookType.EPD),
            
            /*Not used*/
            ConnectionId = 0,
            GitUrl = null!,
            TestBranch = null!,
            TestBranchBench = 0,
            BaseBranch = null!,
            BaseBranchBench = 0,
        }, 4,"""
              -engine proto=uci cmd=/tmp/new/bencher_bin/engine name=new
              -engine proto=uci cmd=/tmp/base/bencher_bin/engine name=base
              -each tc=5.000+0.500
              option.Hash=16
              option.Threads=2
              -rounds 16
              -games 2
              -repeat
              -concurrency 2
              -ratinginterval 0
              -openings order=random file=/tmp/openingbook.epd format=epd
              -recover
              -scoreinterval 0  
              """),
        
        new TestCase("/tmp/openingbook.epd", "/tmp/base", "/tmp/new", 450000, new GetTestNonAutobenchResponse
        {
            ExpectedNps = 500000,   
            HashSize = 64,
            NumberOfThreads = 2,
            TimeManagement = "50+2",
            NumberOfGames = 128,
            AdditionalFastchessOptions = "",
            OpeningBook = new OpeningBookDto("", [], OpeningBookType.EPD),
            
            /*Not used*/
            ConnectionId = 0,
            GitUrl = null!,
            TestBranch = null!,
            TestBranchBench = 0,
            BaseBranch = null!,
            BaseBranchBench = 0,
        }, 17,"""
             -engine proto=uci cmd=/tmp/new/bencher_bin/engine name=new
             -engine proto=uci cmd=/tmp/base/bencher_bin/engine name=base
             -each tc=55.556+2.222
             option.Hash=64
             option.Threads=2
             -rounds 64
             -games 2
             -repeat
             -concurrency 8
             -ratinginterval 0
             -openings order=random file=/tmp/openingbook.epd format=epd
             -recover
             -scoreinterval 0  
             """),
        new TestCase("/tmp/openingbook.epd", "/tmp/base", "/tmp/new", 700000, new GetTestNonAutobenchResponse
        {
            ExpectedNps = 500000,   
            HashSize = 512,
            NumberOfThreads = 4,
            TimeManagement = "60+0.5",
            NumberOfGames = 256,
            AdditionalFastchessOptions = "",
            OpeningBook = new OpeningBookDto("", [], OpeningBookType.EPD),
            
            /*Not used*/
            ConnectionId = 0,
            GitUrl = null!,
            TestBranch = null!,
            TestBranchBench = 0,
            BaseBranch = null!,
            BaseBranchBench = 0,
        }, 64,"""
              -engine proto=uci cmd=/tmp/new/bencher_bin/engine name=new
              -engine proto=uci cmd=/tmp/base/bencher_bin/engine name=base
              -each tc=42.857+0.357
              option.Hash=512
              option.Threads=4
              -rounds 128
              -games 2
              -repeat
              -concurrency 16
              -ratinginterval 0
              -openings order=random file=/tmp/openingbook.epd format=epd
              -recover
              -scoreinterval 0  
              """),
         
        new TestCase("/tmp/openingbook.epd", "/tmp/base", "/tmp/new", 500000, new GetTestNonAutobenchResponse
        {
            ExpectedNps = 500000,   
            HashSize = 16,
            NumberOfThreads = 2,
            TimeManagement = "5+0.50",
            NumberOfGames = 32,
            AdditionalFastchessOptions = "-resign movecount=3 score=500",
            OpeningBook = new OpeningBookDto("", [], OpeningBookType.EPD),
            
            /*Not used*/
            ConnectionId = 0,
            GitUrl = null!,
            TestBranch = null!,
            TestBranchBench = 0,
            BaseBranch = null!,
            BaseBranchBench = 0,
        }, 4,"""
             -engine proto=uci cmd=/tmp/new/bencher_bin/engine name=new
             -engine proto=uci cmd=/tmp/base/bencher_bin/engine name=base
             -each tc=5.000+0.500
             option.Hash=16
             option.Threads=2
             -rounds 16
             -games 2
             -repeat
             -concurrency 2
             -ratinginterval 0
             -openings order=random file=/tmp/openingbook.epd format=epd
             -recover
             -scoreinterval 0
             -resign movecount=3 score=500 
             """),
    ];
    
    /// <summary>
    /// Tests <see cref="FastchessArgumentsCreator.CreateArguments"/>.
    /// </summary>
    [TestCaseSource(nameof(_testCases))]
    public void CreateArguments(TestCase testCase)
    {
        var baseDirectory = new DirectoryInfo(testCase.BaseDirectory);
        var newDirectory = new DirectoryInfo(testCase.NewDirectory);
        testCase = testCase with
        {
            ExpectedArguments = testCase.ExpectedArguments.Replace("\n",  " ")
        };
        
        var arguments = FastchessArgumentsCreator.CreateArguments(testCase.OpeningBookPath,
            baseDirectory,
            newDirectory,
            testCase.BaseNps,
            testCase.Response,
            testCase.ProcessorThreads);
        
        Assert.That(testCase.ExpectedArguments, Is.EqualTo(arguments));
    }
}