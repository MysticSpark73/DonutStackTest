
using UnityEngine;

namespace DonutStack.Core.Pooling
{
    public interface IPoolable
    {
        void OnPool();

        void OnReturn();

        Transform GetTransform();

    }
}