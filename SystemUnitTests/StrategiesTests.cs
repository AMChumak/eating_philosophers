using System.Diagnostics.CodeAnalysis;
using ForkLib;
using Moq;
using StrategyImplementationLib;

namespace SystemUnitTests;

public class StrategiesTests
{
    private Fork _mockLeftFork;
    private Fork _mockRightFork;
    private IForkOwner _stabForkOwner;
    private List<int> _forkIndeces;
    private int _leftIdx = 1;
    private int _rightIdx = 0;

    [SetUp]
    public void Setup()
    {
        var leftForkMock = new Mock<Fork>(20, _leftIdx) {CallBase = true};
        leftForkMock.Setup(f => f.Take(It.IsAny<IForkOwner>()))
            .Callback<IForkOwner>(candidat =>
            {
                _forkIndeces.Add(leftForkMock.Object.OrderNumber);
            });

        var rightForkMock = new Mock<Fork>(20, _rightIdx) {CallBase = true};
        rightForkMock.Setup(f => f.Take(It.IsAny<IForkOwner>()))
            .Callback<IForkOwner>(candidat =>
            {
                _forkIndeces.Add(rightForkMock.Object.OrderNumber);
            });


        _mockLeftFork = leftForkMock.Object;
        _mockRightFork = rightForkMock.Object;

        _forkIndeces = [];
        _stabForkOwner = Mock.Of<IForkOwner>();
    }


    [Test]
    public void TestSimpleStrategy()
    {
        var simpleStrategy = new SimpleTakingForksStrategy();
        simpleStrategy.TakeForksMove(_stabForkOwner, _mockLeftFork, _mockRightFork);
        Assert.That(_forkIndeces[0], Is.EqualTo(_leftIdx));
        Assert.That(_forkIndeces[1], Is.EqualTo(_rightIdx));
    }

    [Test]
    public void TestSourceHierarchyStrategy()
    {
        var simpleStrategy = new SourceHierarchyTakingForksStrategy();
        simpleStrategy.TakeForksMove(_stabForkOwner, _mockLeftFork, _mockRightFork);

        var first = _leftIdx < _rightIdx ? _leftIdx : _rightIdx;
        var second = _leftIdx > _rightIdx ? _leftIdx : _rightIdx;

        Assert.That(_forkIndeces[0], Is.EqualTo(first));
        Assert.That(_forkIndeces[1], Is.EqualTo(second));
    }
}