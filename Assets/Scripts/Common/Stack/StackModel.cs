using DonutStack.Common.Audio;
using DonutStack.Core.MVP.Model;
using DonutStack.Data.Parameters;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DonutStack.Common.Stack
{
    public class StackModel : BaseModel<StackPresenter>
    {
        public int ActiveObjects => stackedObjects.Count(o => o.IsActive == true);

        private IStackedObject[] stackedObjects;
        private Vector3 objectBaseRotation = Vector3.right * -90;

        public StackModel(StackPresenter presenter, IStackedObject[] stackedObjects) : base (presenter)
        {
            this.stackedObjects = stackedObjects;
        }

        public async void GenerateRandomObjects()
        {
            int count = Random.Range(1, 4);
            StackedObjectColor bottomColor = Parameters.GetRandomStackedColor();
            await DeactivateAllObjects(false);
            await AddObject(bottomColor);
            for (int i = 1; i < count; i++)
            {
                await AddObject(Parameters.GetRandomStackedColor(bottomColor));
            }
        }

        public async Task AddObject(StackedObjectColor color, bool animate = true)
        {
            if (ActiveObjects == stackedObjects.Length)
            {
                Debug.LogWarning($"[StackView][AddObject] The Stack is full");
                return;
            }
            stackedObjects[ActiveObjects].SetColor(color);
            await stackedObjects[ActiveObjects].Activate(animate);

            if (GetIsReturning())
            {
                Presenter.SetComplete();
                Presenter.CallOnReturn();
            }
        }

        public async Task RemoveObject(bool animate = true)
        {
            if (ActiveObjects == 0)
            {
                Debug.LogWarning($"[StackView][RemoveObject] The stack is Empty!");
                Presenter.CallOnReturn();
                return;
            }

            for (int i = stackedObjects.Length - 1; i >= 0; i--)
            {
                if (stackedObjects[i].IsActive)
                {
                    await stackedObjects[i].Deactivate(animate);
                    break;
                }
            }

            if (ActiveObjects == 0)
            {
                Presenter.CallOnReturn();
            }
        }

        public async Task DeactivateAllObjects(bool animate = true)
        {
            for (int i = stackedObjects.Length -1; i >=0; i--)
            {
                if (stackedObjects[i].IsActive)
                {
                    await stackedObjects[i].Deactivate(animate);
                    stackedObjects[i].GetTransform().rotation = Quaternion.Euler(objectBaseRotation);
                }
            }
        }

        public StackedObjectColor GetTopObjectColor() 
        {
            for (int i = stackedObjects.Length -1; i >= 0 ; i--)
            {
                if (stackedObjects[i].IsActive)
                {
                    return stackedObjects[i].GetColor();
                }
            }
            //at least one object will be always active
            return StackedObjectColor.Blue;
        }

        public IStackedObject GetTopObject() => stackedObjects[ActiveObjects -1];

        public bool GetIsReturning() 
        {
            if (ActiveObjects == 0)
            {
                return true;
            }
            if (ActiveObjects < 3)
            {
                return false;
            }
            StackedObjectColor color = stackedObjects[0].GetColor();
            for (int i = 1; i < stackedObjects.Length; i++)
            {
                if (stackedObjects[i].GetColor() != color)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
