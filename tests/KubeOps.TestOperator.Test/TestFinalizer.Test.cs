﻿using k8s.Models;
using KubeOps.Testing;
using KubeOps.TestOperator.Entities;
using KubeOps.TestOperator.Finalizer;
using KubeOps.TestOperator.TestManager;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace KubeOps.TestOperator.Test
{
    public class TestFinalizerTest : IClassFixture<KubernetesOperatorFactory<TestStartup>>
    {
        private readonly KubernetesOperatorFactory<TestStartup> _factory;

        public TestFinalizerTest(KubernetesOperatorFactory<TestStartup> factory)
        {
            _factory = factory.WithSolutionRelativeContentRoot("tests/KubeOps.TestOperator");
        }

        [Fact]
        public void Test_If_Manager_Finalized_Is_Called()
        {
            _factory.Run();
            var mock = _factory.Services.GetRequiredService<Mock<IManager>>();
            mock.Reset();
            mock.Setup(o => o.Finalized(It.IsAny<V1TestEntity>()));
            mock.Verify(o => o.Finalized(It.IsAny<V1TestEntity>()), Times.Never);
            _factory.MockedKubernetesClient.UpdateResult = new V1TestEntity();
            var queue = _factory.GetMockedEventQueue<V1TestEntity>();
            queue.Finalizing(
                new V1TestEntity
                {
                    Metadata = new V1ObjectMeta
                    {
                        Finalizers = new[] { new TestEntityFinalizer(mock.Object, null, null).Identifier },
                    },
                });
            mock.Verify(o => o.Finalized(It.IsAny<V1TestEntity>()), Times.Once);
        }
    }
}
