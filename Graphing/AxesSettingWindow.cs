﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphing
{
    /// <summary>
    /// A class that generates a KSP window that allows users to set custom
    /// bounds for the graph.
    /// </summary>
    public sealed class AxesSettingWindow
    {
        /// <summary>
        /// The <see cref="Grapher"/> that is the parent of this window and
        /// that will be affected by any changes.
        /// </summary>
        public readonly Grapher grapher;

        private string xMinStr, xMaxStr, yMinStr, yMaxStr, zMinStr, zMaxStr;
        private float setXmin, setXmax, setYmin, setYmax, setZmin, setZmax;
        private bool[] useSelfAxesToggle = new bool[] { true, true, true };

        DialogGUITextInput xMinInput;
        DialogGUITextInput xMaxInput;
        DialogGUITextInput yMinInput;
        DialogGUITextInput yMaxInput;
        DialogGUITextInput zMinInput;
        DialogGUITextInput zMaxInput;

        /// <summary>
        /// Constructs a new <see cref="AxesSettingWindow"/>.
        /// </summary>
        /// <param name="grapher">
        /// The <see cref="Grapher"/> that this window will be attached to.
        /// </param>
        public AxesSettingWindow(Grapher grapher)
        {
            this.grapher = grapher;
        }

        /// <summary>
        /// Spawns the window. This will destroy an existing window from this object.
        /// </summary>
        /// <returns>The KSP <see cref="PopupDialog"/> spawned.</returns>
        public PopupDialog SpawnPopupDialog()
        {
            useSelfAxesToggle[0] = grapher.useSelfAxes[0];
            useSelfAxesToggle[1] = grapher.useSelfAxes[1];
            useSelfAxesToggle[2] = grapher.useSelfAxes[2];
            OnAutoToggle(0, useSelfAxesToggle[0]);
            OnAutoToggle(1, useSelfAxesToggle[1]);
            OnAutoToggle(2, useSelfAxesToggle[2]);

            xMinInput = new DialogGUITextInput(xMinStr, false, 10, (str) => xMinStr = str, 25) { OptionInteractableCondition = () => !useSelfAxesToggle[0] };
            xMaxInput = new DialogGUITextInput(xMaxStr, false, 10, (str) => xMaxStr = str, 25) { OptionInteractableCondition = () => !useSelfAxesToggle[0] };
            yMinInput = new DialogGUITextInput(yMinStr, false, 10, (str) => yMinStr = str, 25) { OptionInteractableCondition = () => !useSelfAxesToggle[1] };
            yMaxInput = new DialogGUITextInput(yMaxStr, false, 10, (str) => yMaxStr = str, 25) { OptionInteractableCondition = () => !useSelfAxesToggle[1] };
            zMinInput = new DialogGUITextInput(zMinStr, false, 10, (str) => zMinStr = str, 25) { OptionInteractableCondition = () => !useSelfAxesToggle[2] };
            zMaxInput = new DialogGUITextInput(zMaxStr, false, 10, (str) => zMaxStr = str, 25) { OptionInteractableCondition = () => !useSelfAxesToggle[2] };

            grapher.ValueChangedExternally += OnValueChanged;

            List<DialogGUIBase> dialog = new List<DialogGUIBase>
            {
                //new DialogGUIToggleButton(() => useSelfAxesToggle[0] && useSelfAxesToggle[1] && useSelfAxesToggle[2], "Auto Axes", (value) => useSelfAxesToggle[0] = useSelfAxesToggle[1] = useSelfAxesToggle[2] = value, h: 20),
                //new DialogGUISpace(5),
                new DialogGUILabel("X-Axis"),
                new DialogGUIToggle(() => useSelfAxesToggle[0], "Auto", (value) => { OnAutoToggle(0, value); SetText(xMinInput, xMinStr); SetText(xMaxInput, xMaxStr); }, h: 20),
                new DialogGUIHorizontalLayout(new DialogGUILabel(String.Format("Min:\t")), xMinInput),
                new DialogGUIHorizontalLayout(new DialogGUILabel(String.Format("Max:\t")), xMaxInput),
                new DialogGUISpace(5),
                new DialogGUILabel("Y-Axis"),
                new DialogGUIToggle(() => useSelfAxesToggle[1], "Auto", (value) => { OnAutoToggle(1, value); SetText(yMinInput, yMinStr); SetText(yMaxInput, yMaxStr); }, h: 20),
                new DialogGUIHorizontalLayout(new DialogGUILabel(String.Format("Min:\t")), yMinInput),
                new DialogGUIHorizontalLayout(new DialogGUILabel(String.Format("Max:\t")), yMaxInput),
                new DialogGUISpace(5),
                new DialogGUIButton("Apply", () =>
                {
                    float[] temp = new float[6];
                    if (float.TryParse(xMinStr, out temp[0]) && float.TryParse(xMaxStr, out temp[1]))
                    {
                        setXmin = temp[0]; setXmax = temp[1];
                    }
                    if (float.TryParse(yMinStr, out temp[2]) && float.TryParse(yMaxStr, out temp[3]))
                    {
                        setYmin = temp[2]; setYmax = temp[3];
                    }
                    if (float.TryParse(zMinStr, out temp[4]) && float.TryParse(zMaxStr, out temp[5]))
                    {
                        setZmin = temp[4]; setZmax = temp[5];
                    }

                    if (!useSelfAxesToggle[0])
                        grapher.SetAxesLimits(0, setXmin, setXmax, true);
                    else
                        grapher.ReleaseAxesLimits(0, true);
                    if (!useSelfAxesToggle[1])
                        grapher.SetAxesLimits(1, setYmin, setYmax, true);
                    else
                        grapher.ReleaseAxesLimits(1, true);
                    if (!useSelfAxesToggle[2])
                        grapher.SetAxesLimits(2, setZmin, setZmax, true);
                    else
                        grapher.ReleaseAxesLimits(2, true);

                    if (grapher.RecalculateLimits())
                        grapher.OnAxesChanged();
                }, false),
                new DialogGUIButton("Close", () => { grapher.ValueChangedExternally -= OnValueChanged; }, true)
            };
            
            if (grapher.Any(g => g is IGraphable3))
            {
                dialog.InsertRange(10, new List<DialogGUIBase>
                {
                    new DialogGUILabel("Z-Axis"),
                    new DialogGUIToggle(() => useSelfAxesToggle[2], "Auto", (value) => { OnAutoToggle(2, value); SetText(zMinInput, zMinStr); SetText(zMaxInput, zMaxStr); }, h: 20),
                    new DialogGUIHorizontalLayout(new DialogGUILabel(String.Format("Min:\t")), zMinInput),
                    new DialogGUIHorizontalLayout(new DialogGUILabel(String.Format("Max:\t")), zMaxInput),
                    new DialogGUISpace(5)
                });
            }

            return PopupDialog.SpawnPopupDialog(new Vector2(0, 1), new Vector2(0, 1),
                new MultiOptionDialog("KWTAxesSettings", "", "Axes Settings", UISkinManager.defaultSkin, 150,
                    dialog.ToArray()),
                false, UISkinManager.defaultSkin);
        }
        private void OnAutoToggle(int axis, bool value)
        {
            useSelfAxesToggle[axis] = value;
            switch (axis)
            {
                case 0:
                    if (value)
                    {
                        setXmin = grapher.selfXmin;
                        setXmax = grapher.selfXmax;
                    }
                    else
                    {
                        setXmin = grapher.setXmin;
                        setXmax = grapher.setXmax;
                    }
                    xMinStr = setXmin.ToString();
                    xMaxStr = setXmax.ToString();
                    break;
                case 1:
                    if (value)
                    {
                        setYmin = grapher.selfYmin;
                        setYmax = grapher.selfYmax;
                    }
                    else
                    {
                        setYmin = grapher.setYmin;
                        setYmax = grapher.setYmax;
                    }
                    yMinStr = setYmin.ToString();
                    yMaxStr = setYmax.ToString();
                    break;
                case 2:
                    if (value)
                    {
                        setZmin = grapher.selfZmin;
                        setZmax = grapher.selfZmax;
                    }
                    else
                    {
                        setZmin = grapher.setZmin;
                        setZmax = grapher.setZmax;
                    }
                    zMinStr = setZmin.ToString();
                    zMaxStr = setZmax.ToString();
                    break;
            }
        }

        private static void SetText(DialogGUITextInput field, string text)
        {
            field.uiItem.GetComponent<TMPro.TMP_InputField>().text = text;
        }

        private void OnValueChanged(object sender, ExternalValueChangeEventArgs eventArgs)
        {
            if (sender != grapher)
                return;

            switch (eventArgs.AxisIndex)
            {
                case 0: SetText(xMinInput, eventArgs.Min.ToString()); SetText(xMaxInput, eventArgs.Max.ToString()); break;
                case 1: SetText(yMinInput, eventArgs.Min.ToString()); SetText(yMaxInput, eventArgs.Max.ToString()); break;
                case 2: SetText(zMinInput, eventArgs.Min.ToString()); SetText(zMaxInput, eventArgs.Max.ToString()); break;
            }
        }
    }
}
