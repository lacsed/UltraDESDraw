using AutoAVL;
using AutoAVL.Drawables;
using Microsoft.AspNetCore.Components.Web;

namespace UltraDESDraw.Services 
{
    public class SharedDataService
    {
        public event Action OnChange;
        public string SomeData {get; set; } = "Default";
        public Graph Graph {get; set; } = new Graph();
        public Node? SelectedNode {get; set; }
        public Node? InsideNode {get; set; }

        public Vector2D? TempLinkStart {get; set; }
        public Vector2D? TempLinkEnd {get; set; }
        public Node? StartNode {get; set; }
        public Node? EndNode {get; set; }

        #region Variáveis de estado da canvas.
        private string cursor = "cursor: default;";
        private bool _panning = false;
        private bool _movingNode = false;
        private bool _creatingLink = false;
        private bool _initialTransition = false;
        private bool _holdingSpace = false;
        private bool _holdingShift = false;
        private bool _mouseDown = false;
        private Vector2D _mousePosition = new Vector2D();
        #endregion

        public void NotifyDataChanged() => OnChange?.Invoke();

        // Access functions to private variables.

        public bool GetIsCreatingLink()
        {
            return _creatingLink;
        }

        public string GetCursorType()
        {
            return cursor;
        }

        public bool IsCreatingInitialTransition()
        {
            return _initialTransition;
        }

        private void ClearActions()
        {
            _panning = false;
            _movingNode = false;
            _creatingLink = false;
            _initialTransition = false;
        }

        private void ClearLinkData()
        {
            StartNode = null;
            EndNode = null;
            TempLinkStart = null;
            TempLinkEnd = null;
        }

        public void NodeEnter(Node node)
        {
            InsideNode = node;

            if (node == StartNode)
                return;

            if (_initialTransition)
            {
                EndNode = node;
            } else if (StartNode != null)
            {
                EndNode = node;
            }

            NotifyDataChanged();
        }
        public void NodeLeave(Node node)
        {
            if (EndNode == node)
                EndNode = null;

            InsideNode = null;

            NotifyDataChanged();
        }
        public void CanvasMouseDownEvent(MouseEventArgs e)
        {
            if (e.Button == 0)
            {
                _mouseDown = true;

                if (_holdingSpace)
                {
                    _panning = true;
                    cursor = "cursor: grabbing;";
                } else if (e.ShiftKey)
                {
                    _creatingLink = true;

                    if (InsideNode == null)
                    {
                        _initialTransition = true;
                        TempLinkStart = _mousePosition;
                    } else
                        StartNode = InsideNode;

                    TempLinkEnd = _mousePosition;
                    
                } else if (InsideNode != null)
                {
                    _movingNode = true;
                    cursor = "cursor: move;";
                }
            }

            NotifyDataChanged();
        }

        public void CanvasMouseUpEvent(MouseEventArgs e)
        {
            if (e.Button == 0)
            {
                _mouseDown = false;

                if (_panning)
                {
                    _panning = false;
                    cursor = "cursor: grab;";
                } 
                else if (_creatingLink)
                {
                    _creatingLink = false;
                    _initialTransition = false;
                    
                    if (EndNode != null)
                    {
                        if (Graph?.graphLinks != null)
                        {
                            if (StartNode != null)
                            {
                                var newLink = new Link(StartNode, EndNode, "");
                                Graph.graphLinks.Add(newLink);
                            } else if (_initialTransition)
                            {
                                var previousInitialLink = Graph.graphLinks.Find(x => x.isInitialLink);
                                if (previousInitialLink != null)
                                    Graph.graphLinks.Remove(previousInitialLink);
                                var newLink = new Link(EndNode);
                                Graph.graphLinks.Add(newLink);
                            }
                        }
                    }
                } else if (_movingNode)
                {
                    cursor = "cursor: default;";
                }

                ClearActions();
                ClearLinkData();
                NotifyDataChanged();
            }
        }

        public void CanvasKeyDownEvent(KeyboardEventArgs e)
        {
            if (e.Key == " " && !_panning)
            {
                _holdingSpace = true;
                cursor = "cursor: grab;";
                NotifyDataChanged();
            }
            else if (e.Key == "Shift")
            {
                _holdingShift = true;
            }
        }

        public void CanvasKeyUpEvent(KeyboardEventArgs e)
        {
            if (e.Key == " ")
            {
                _holdingSpace = false;
                cursor = "cursor: default;";
                _panning = false;
                NotifyDataChanged();
            }
            else if (e.Key == "Shift")
            {
                _holdingShift = false;
            }
        }

        public void CanvasMouseMoveEvent(MouseEventArgs e)
        {
            _mousePosition = new Vector2D((float) e.OffsetX, (float) e.OffsetY);

            if (_panning)
            {
                Graph.svgCanvas.MoveOrigin(new Vector2D((float) -e.MovementX, (float) e.MovementY));
            }
            else if (_movingNode)
            {
                SelectedNode.position += new Vector2D((float) e.MovementX, (float) -e.MovementY);
            }
            else if (_creatingLink)
            {
                TempLinkEnd = _mousePosition;
            }

            NotifyDataChanged();
        }
    }
}