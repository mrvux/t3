﻿using System;
using System.Collections.Generic;
using ImGuiNET;
using SharpDX;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Gui.Graph.Interaction;
using T3.Gui.OutputUi;
using T3.Gui.Selection;
using T3.Gui.Styling;
using Vector2 = System.Numerics.Vector2;

namespace T3.Gui.Windows.Output
{
    public class OutputWindow : Window
    {
        #region Window implementation
        public OutputWindow()
        {
            Config.Title = "Output##" + _instanceCounter;
            Config.Visible = true;

            AllowMultipleInstances = true;
            Config.Visible = true;
            WindowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

            _instanceCounter++;
            OutputWindowInstances.Add(this);
        }

        protected override void DrawAllInstances()
        {
            // Convert to array to enable removable of members during iteration
            foreach (var w in OutputWindowInstances.ToArray())
            {
                w.DrawOneInstance();
            }
        }

        protected override void Close()
        {
            OutputWindowInstances.Remove(this);
        }

        protected override void AddAnotherInstance()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new OutputWindow();
        }

        public override List<Window> GetInstances()
        {
            return OutputWindowInstances;
        }
        #endregion

        protected override void DrawContent()
        {
            ImGui.BeginChild("##content", new Vector2(0, ImGui.GetWindowHeight()), false,
                             ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollWithMouse);
            {
                _imageCanvas.SetAsCurrent();
                _imageCanvas.PreventMouseInteraction = CameraSelectionHandling.SelectedCamera != null;
                _imageCanvas.Update();

                if(!SelectionManager._isGizmoDragging)
                    _cameraInteraction.Update(CameraSelectionHandling.SelectedCamera);

                // move down to avoid overlapping with toolbar
                ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin() + new Vector2(0, 40));
                DrawOutput(_pinning.GetPinnedOrSelectedInstance(), _pinning.GetPinnedEvaluationInstance());
                _imageCanvas.Deactivate();

                DrawToolbar();
            }
            ImGui.EndChild();
        }

        public Instance ShownInstance => _pinning.GetPinnedOrSelectedInstance();

        private void DrawToolbar()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Color(0.6f).Rgba);
            ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin());
            _pinning.DrawPinning();
            
            ImGui.PushStyleColor(ImGuiCol.Text, Math.Abs(_imageCanvas.Scale.X - 1f) < 0.001f ? Color.Black.Rgba: Color.White);
            if (ImGui.Button("1:1"))
            {
                _imageCanvas.SetScaleToMatchPixels();
                _imageCanvas.SetViewMode(ImageOutputCanvas.Modes.Pixel);
            }
            ImGui.PopStyleColor();

            ImGui.SameLine();
            
            ImGui.PushStyleColor(ImGuiCol.Text, _imageCanvas.ViewMode == ImageOutputCanvas.Modes.Fitted ? Color.Black.Rgba: Color.White);
            if (ImGui.Button("Fit") || KeyboardBinding.Triggered(UserActions.FocusSelection))
            {
                _imageCanvas.SetViewMode(ImageOutputCanvas.Modes.Fitted);
            }
            ImGui.PopStyleColor();

            ImGui.SameLine();

            CameraSelectionHandling.DrawCameraSelection(_pinning, ref _selectedCameraId);
            ResolutionHandling.DrawSelector(ref _selectedResolution, _resolutionDialog);
            ImGui.PopStyleColor();
        }

        private void DrawOutput(Instance instanceForOutput, Instance instanceForEvaluation= null)
        {
            if (instanceForEvaluation == null)
                instanceForEvaluation = instanceForOutput;
                    
            if (instanceForEvaluation == null || instanceForEvaluation.Outputs.Count <= 0)
                return;

            var evaluatedSymbolUi = SymbolUiRegistry.Entries[instanceForEvaluation.Symbol.Id];

            // Todo: use output from pinning...
            var evalOutput = instanceForEvaluation.Outputs[0];
            if (!evaluatedSymbolUi.OutputUis.TryGetValue(evalOutput.Id, out IOutputUi evaluatedOutputUi))
                return;

            _evaluationContext.Reset();
            _evaluationContext.RequestedResolution = _selectedResolution.ComputeResolution();
            
            
            // Ugly hack to hide final target
            if (instanceForOutput != instanceForEvaluation)
            {
                ImGui.BeginChild("hidden", Vector2.One * 1);
                {
                    evaluatedOutputUi.DrawValue(evalOutput, _evaluationContext);
                }
                ImGui.EndChild();

                if (instanceForOutput == null || instanceForOutput.Outputs.Count == 0)
                    return;
                    
                var viewOutput = instanceForOutput.Outputs[0];
                var viewSymbolUi = SymbolUiRegistry.Entries[instanceForOutput.Symbol.Id];
                if (!viewSymbolUi.OutputUis.TryGetValue(viewOutput.Id, out IOutputUi viewOutputUi))
                    return;

                viewOutputUi.DrawValue(viewOutput, _evaluationContext, recompute:false);    
            }
            else
            {
                evaluatedOutputUi.DrawValue(evalOutput, _evaluationContext);
            }
        }

        private readonly EvaluationContext _evaluationContext = new EvaluationContext();
        public static readonly List<Window> OutputWindowInstances = new List<Window>();
        private readonly ImageOutputCanvas _imageCanvas = new ImageOutputCanvas();
        private readonly ViewSelectionPinning _pinning = new ViewSelectionPinning();
        private readonly CameraInteraction _cameraInteraction = new CameraInteraction();

        private Guid _selectedCameraId = Guid.Empty;
        private static int _instanceCounter;
        private ResolutionHandling.Resolution _selectedResolution = ResolutionHandling.DefaultResolution;

        private readonly EditResolutionDialog _resolutionDialog = new EditResolutionDialog();
    }
}