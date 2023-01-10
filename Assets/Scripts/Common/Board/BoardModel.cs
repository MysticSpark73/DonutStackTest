using DonutStack.Common.Stack;
using DonutStack.Core.MVP.Model;
using DonutStack.Data.Parameters;
using System.Collections.Generic;
using DonutStack.Common.Events;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using DonutStack.Data.Score;
using DonutStack.Data;
using DonutStack.Common.Audio;

namespace DonutStack.Common.Board
{
    public class BoardModel : BaseModel<BoardPresenter>
    {
        //I assume that rows number is always odd
        public Vector2 TouchPos { get; private set; }
        public int RowsNumber { get; set; }
        public int CollumnsNumber { get; set; }
        public int CurrentRow { get; set; }
        private int HalfRows => (RowsNumber - 1) / 2;
        private int HalfCollumns => (CollumnsNumber - 1) / 2;
        private bool IsTouching { get; set; }

        private List<MatrixStack> stacksOnBoard;
        private Vector3 spawnOrigin = Vector3.forward * -4 + Vector3.up * .05f;
        private StackView currentObject;
        private Transform currentObjectTransform;
        private StackView awaitedPushedStack;
        private StackView awaitedReturnedStack;
        private ScoreManager scoreManager;

        public BoardModel(BoardPresenter presenter) : base(presenter)
        {
            EventManager.OnTouchDown += OnTouchDown;
            EventManager.OnTouchUp += OnTouchUp;
            EventManager.OnStackStop += OnStackStop;
            EventManager.OnStackRemoved += OnStackRemoved;
            EventManager.OnGameStateChanged += OnGameStateChanged;
            EventManager.OnGameRestart += OnGameRestart;
            stacksOnBoard = new List<MatrixStack>();
            scoreManager = new ScoreManager();
            scoreManager.CallReset();
            Parameters.SetGameState(GameState.Playing);
            SpawnStack();
        }

        public void OnQuit()
        {
            EventManager.OnTouchDown -= OnTouchDown;
            EventManager.OnTouchUp -= OnTouchUp;
            EventManager.OnStackStop -= OnStackStop;
            EventManager.OnStackRemoved -= OnStackRemoved;
            EventManager.OnGameStateChanged -= OnGameStateChanged;
            EventManager.OnGameRestart -= OnGameRestart;
        }

        private void SetCurrentRow(Vector2 pos)
        {
            CurrentRow = IsTouching ? Mathf.Clamp(SnapScreenToRows(pos), 0, RowsNumber - 1) : -1;
            Presenter.SetCurrentRow();
        }

        private int SnapScreenToRows(Vector2 screenPos)
        {
            return (int)screenPos.x / (Screen.width / RowsNumber);
        }

        private void ClearBoard() {
            foreach (var e in stacksOnBoard)
            {
                e.stack.OnReturn();
            }
        }

        public void SpawnStack()
        {
            currentObject = Presenter.SpawnStack(Parameters.object_pooler_key_stack, spawnOrigin);
            if (currentObject == null)
            {
                Debug.LogError($"Pooled object is null!!");
                return;
            }
            currentObjectTransform = currentObject.GetTransform();
        }

        private void PushStack(StackView stack)
        {
            if (stack == null)
            {
                Debug.LogError($"[BoardModel][ThrowStack] stack is null can not throw");
                return;
            }

            Rigidbody rb = stack.GetRigidbody();
            if (rb == null)
            {
                Debug.LogError($"[BoardModel][ThrowStack] obj {currentObject} do not have a Rigidbody component!");
                return;
            }
            rb.isKinematic = false;
            rb.velocity = Vector3.forward * 10;
            stack.SetMoving();
            MatrixStack find = GetStack(stack);
            if (find.stack == stack)
            {
                return;
            }
            ClearCurrentObject();
        }

        private async Task MergeAndFall(StackView stack)
        {
            await MergeStacks(stack);
            List<StackView> fallenStacks = await WaitStacksFall();
            if (fallenStacks.Count == 0)
            {
                return;
            }
            foreach (var s in fallenStacks)
            {
                await MergeAndFall(s);
            }
        }

        private async Task<List<StackView>> WaitStacksFall()
        {
            List<StackView> fallenStacks = new List<StackView>();
            foreach (var stack in stacksOnBoard)
            {
                if (IsStackFalling(stack.stack))
                {
                    fallenStacks.Add(stack.stack);
                }
            }
            foreach (var fallenStack in fallenStacks)
            {
                PushStack(fallenStack);
                await WaitUntilStackStop(fallenStack);
            }
            return fallenStacks;
        }

        private async Task MergeStacks(StackView arrivedStack, StackView exclude = null)
        {
            //init values
            StackView interactedNeighbor = null;
            MatrixStack target = GetStack(arrivedStack);
            List<StackView> neighbors = FindNeighbors(target);
            StackedObjectColor oldColor;

            if (target.stack == null)
            {
                Debug.LogError($"[BoardModel][MergeStacks] target is null on {arrivedStack}");
                return;
            }

            if (neighbors.Count == 0)
            {
                Debug.LogError($"[BoardModel][MergeStacks] Stack {arrivedStack} has no neighbours");
                return;
            }

            //get color of top object
            StackedObjectColor topColor = target.stack.GetTopObjectColor();
            oldColor = topColor;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == exclude)
                {
                    Debug.LogWarning($"[BoardModel][MergeStacks] stack {neighbor} excluded");
                    continue;
                }
                if (neighbor.GetTopObjectColor() == topColor)
                {
                    //if neighbor is small throw one object on it
                    if (neighbor.GetActiveObjectsCount() < 3)
                    {
                        await arrivedStack.ThrowAnim(neighbor);
                        interactedNeighbor = neighbor;

                        if (arrivedStack.GetIsReturning() == true) { break; }
                        if (neighbor.GetIsReturning())
                        {
                            await WaitUntilStackRemoved(neighbor);
                            return;
                        }

                        topColor = arrivedStack.GetTopObjectColor();
                        //if can throw one more, do it
                        if (topColor == neighbor.GetTopObjectColor() && neighbor.GetActiveObjectsCount() < 3)
                        {
                            await arrivedStack.ThrowAnim(neighbor);
                        }
                        break;
                    }
                    //if neighbor is full and stack have unfilled anchors, throw object from neighbor on it
                    if (neighbor.GetActiveObjectsCount() == 3 && arrivedStack.GetActiveObjectsCount() < 3)
                    {
                        await neighbor.ThrowAnim(arrivedStack);
                        interactedNeighbor = neighbor;
                        //throw second one if can
                        if (neighbor.GetTopObjectColor() == topColor && arrivedStack.GetActiveObjectsCount() < 3)
                        {
                            await neighbor.ThrowAnim(arrivedStack);
                        }
                        break;
                    }
                }
            }
            //if any stacks now full or empty wait until they go
            if (arrivedStack.GetIsReturning())
            {
                await WaitUntilStackRemoved(arrivedStack);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.GetIsReturning())
                    {
                        await WaitUntilStackRemoved(neighbor);
                        return;
                    }
                    if (neighbor == exclude)
                    {
                        continue;
                    }
                    //recursive call for each neighbor
                    await MergeStacks(neighbor, arrivedStack);
                }
                return;
            }
            if (interactedNeighbor != null)
            {
                if (interactedNeighbor.GetIsReturning())
                {
                    await WaitUntilStackRemoved(interactedNeighbor);
                }
                neighbors = FindNeighbors(target);
            }
            //if stack falling return up to MergeAndFall 
            if (IsStackFalling(arrivedStack))
            {
                return;
            }
            topColor = arrivedStack.GetTopObjectColor();
            if (oldColor != topColor)
            {
                Debug.LogWarning($"[BoardModel][MergeStacks] {arrivedStack} recursive call. New color is {topColor}, old color is {oldColor}");
                //recursive call if color changed
                await MergeStacks(arrivedStack);
                return;
            }
            if (interactedNeighbor == null) {return;}
            //recursive call for each neighbor excluding self
            foreach (var neighbor in neighbors)
            {
                Debug.LogWarning($"[BoardModel][MergeStacks] {arrivedStack} asking neighbours");
                if (neighbor == exclude)
                {
                    continue;
                }
                await MergeStacks(neighbor, arrivedStack);

            }
        }

        private List<StackView> FindNeighbors(MatrixStack target)
        {
            List<StackView> res = new List<StackView>();
            
            foreach (var stack in stacksOnBoard)
            {
                if (stack.pos.x == target.pos.x &&
                    (stack.pos.y == target.pos.y + 1 || stack.pos.y == target.pos.y -1))
                {
                    res.Add(stack.stack);
                }
                if (stack.pos.y == target.pos.y && 
                    (stack.pos.x == target.pos.x +1 || stack.pos.x == target.pos.x -1))
                {
                    res.Add(stack.stack);
                }
            }
            return res;
        }

        private bool IsStackFalling(StackView stack)
        {
            MatrixStack matrix = GetStack(stack);
            if (matrix.stack == null)
            {
                Debug.LogError($"[BoardModel][IsStackFalling] stack {stack} is not on board!");
                return false;
            }
            RaycastHit[] hits = Physics.RaycastAll(matrix.stack.GetTransform().position, Vector3.forward, .8f);
            Debug.DrawRay(matrix.stack.GetTransform().position, Vector3.forward, Color.red, 1);
           
            if (hits.Length == 0)
            {
                return true;
            }
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject != stack.gameObject)
                {
                    return false;
                }
            }
            return true;
            
        }

        private MatrixStack GetStack(StackView stack) 
        {
            return stacksOnBoard.FirstOrDefault(s => s.stack == stack && s.stack.IsActive);
        }

        private void UpdateObject(MatrixStack matrix) 
        {
            int index = -1;
            for (int i = 0; i < stacksOnBoard.Count; i++)
            {
                if (stacksOnBoard[i].stack == matrix.stack)
                {
                    index = i;
                }
            }
            if (index == -1)
            {
                Debug.LogError($"[BoardModel][UpdateObject] Cant find target element {matrix}");
                return;
            }
            stacksOnBoard.RemoveAt(index);
            stacksOnBoard.Add(matrix);
        }

        private Vector2Int CalculateGridPos(Vector3 pos) 
        {
            Vector2Int roundedPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
            return (roundedPos + new Vector2Int(HalfRows, -HalfCollumns)) * new Vector2Int(1, -1);
        }

        private void ClearCurrentObject() 
        {
            currentObject = null;
            currentObjectTransform = null;
        }

        private bool IsGameFailed() 
        {
            return stacksOnBoard.Any(s => s.pos.y == CollumnsNumber - 1);
        }

        #region Events

        private void OnTouchDown(Vector2 pos)
        {
            IsTouching = true;
            TouchPos = pos;
            
            if (currentObjectTransform == null)
            {
                Debug.LogError($"[BoardModel][OnTouchMove] missing object transform. Can not move! {currentObject}");
                return;
            }
            SetCurrentRow(TouchPos);
            currentObjectTransform.position = spawnOrigin + Vector3.right * Mathf.Clamp((SnapScreenToRows(pos) - HalfRows), -HalfRows, HalfRows);
        }

        private void OnTouchUp()
        {
            IsTouching = false;
            SetCurrentRow(TouchPos);
            PushStack(currentObject);
            InputManager.LockTouch();
        }

        private async void OnStackStop(StackView stack)
        {
            MatrixStack find = GetStack(stack);
            if (find.stack == stack)
            {
                Vector2Int newPos = CalculateGridPos(stack.GetTransform().position);
                find.stack.gridPos = CalculateGridPos(stack.GetTransform().position);
                UpdateObject(new MatrixStack(stack, newPos));
                if (awaitedPushedStack == stack)
                {
                    awaitedPushedStack = null;
                }
                return;
            }
            else
            {
                stacksOnBoard.Add(new MatrixStack(stack, CalculateGridPos(stack.GetTransform().position)));
                stack.gridPos = CalculateGridPos(stack.GetTransform().position);
            }
            await stack.StopAnim();
            await MergeAndFall(stack);
            if (IsGameFailed())
            {
                Parameters.SetGameState(GameState.GameEnd);
                AudioController.Instance.PlaySound(AudioController.Sounds.LoseSound);
            }
            if (Parameters.gameState == GameState.Playing)
            {
                SpawnStack();
            }
            await new WaitForSeconds(1f);
            InputManager.UnlockTouch();
        }

        private void OnStackRemoved(StackView stack) 
        {
            MatrixStack obj = GetStack(stack);
            if (stack == awaitedReturnedStack)
            {
                awaitedReturnedStack = null;
            }
            if (Parameters.gameState == GameState.Playing)
            {
                if (stack.GetComplete())
                {
                    scoreManager.AddCompleteStackScore();
                }
                else
                {
                    scoreManager.AddEmptyStackScore();
                }
            }
            if (!stacksOnBoard.Contains(obj))
            {
                Debug.Log($"[BoardModel][OnStackRemove] Given stack {stack} do not excist on the board!");
                return;
            }
            stacksOnBoard.Remove(obj);
        }

        private void OnGameStateChanged(GameState gameState) 
        {
            switch (gameState)
            {
                case GameState.Playing:
                    InputManager.UnlockTouch();
                    break;
                case GameState.GameEnd:
                    InputManager.LockTouch();
                    break;
                case GameState.Menu:
                    break;
                case GameState.Loading:
                    break;
                default:
                    break;
            }
        }

        private async void OnGameRestart() 
        {
            ClearBoard();
            await new WaitForSeconds(2);
            scoreManager.CallReset();
            Parameters.SetGameState(GameState.Playing);
            SpawnStack();
        }

        private async Task WaitUntilStackStop(StackView stackView)
        {
            awaitedPushedStack = stackView;
            await new WaitUntil(() => awaitedPushedStack == null);
        }

        private async Task WaitUntilStackRemoved(StackView stackView)
        {
            awaitedReturnedStack = stackView;
            await new WaitUntil(() => awaitedReturnedStack == null);
        }

        #endregion

        [System.Serializable]
        public struct MatrixStack
        {
            public StackView stack;
            public Vector2Int pos;

            public MatrixStack(StackView stack, Vector2Int pos)
            {
                this.stack = stack;
                this.pos = pos;
            }
        }

    }
}
