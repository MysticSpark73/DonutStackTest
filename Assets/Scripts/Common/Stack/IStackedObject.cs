using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DonutStack.Common.Stack
{
    public interface IStackedObject
    {
        public bool IsActive { get; set; }

        void Init(StackedObjectColor color, Action callback = null);

        Transform GetTransform();

        StackedObjectColor GetColor();

        void SetColor(StackedObjectColor color);

        Task Activate(bool animate = true);

        Task Deactivate(bool animate = true);
    }
}