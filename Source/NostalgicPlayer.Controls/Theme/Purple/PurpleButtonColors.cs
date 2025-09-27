/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple
{
    /// <summary>
    /// Different purple inspired colors used by buttons
    /// </summary>
    internal class PurpleButtonColors : IButtonColors
    {
        private static readonly Color normalBorderColor = Color.FromArgb(140, 125, 160);
        private static readonly Color normalBackgroundStartColor = Color.FromArgb(230, 225, 235);
        private static readonly Color normalBackgroundStopColor = Color.FromArgb(210, 200, 230);
        private static readonly Color normalTextColor = Color.FromArgb(55, 30, 85);

        private static readonly Color hoverBorderColor = Color.FromArgb(90, 100, 205);
        private static readonly Color hoverBackgroundStartColor = Color.FromArgb(190, 195, 250);
        private static readonly Color hoverBackgroundStopColor = Color.FromArgb(165, 170, 200);
        private static readonly Color hoverTextColor = Color.FromArgb(55, 30, 85);

        private static readonly Color pressedBorderColor = Color.FromArgb(110, 90, 140);
        private static readonly Color pressedBackgroundStartColor = Color.FromArgb(195, 183, 215);
        private static readonly Color pressedBackgroundStopColor = Color.FromArgb(185, 170, 205);
        private static readonly Color pressedTextColor = Color.FromArgb(55, 30, 85);

        private static readonly Color focusedBorderColor = Color.FromArgb(255, 255, 255);
        private static readonly Color focusedBackgroundStartColor = Color.FromArgb(235, 230, 240);
        private static readonly Color focusedBackgroundStopColor = Color.FromArgb(225, 215, 242);
        private static readonly Color focusedTextColor = Color.FromArgb(55, 30, 85);

        private static readonly Color disabledBorderColor = Color.FromArgb(180, 180, 180);
        private static readonly Color disabledBackgroundStartColor = Color.FromArgb(235, 235, 235);
        private static readonly Color disabledBackgroundStopColor = Color.FromArgb(235, 235, 235);
        private static readonly Color disabledTextColor = Color.FromArgb(168, 168, 168);

        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color NormalBorderColor => normalBorderColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color NormalBackgroundStartColor => normalBackgroundStartColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color NormalBackgroundStopColor => normalBackgroundStopColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color NormalTextColor => normalTextColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color HoverBorderColor => hoverBorderColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color HoverBackgroundStartColor => hoverBackgroundStartColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color HoverBackgroundStopColor => hoverBackgroundStopColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color HoverTextColor => hoverTextColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color PressedBorderColor => pressedBorderColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color PressedBackgroundStartColor => pressedBackgroundStartColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color PressedBackgroundStopColor => pressedBackgroundStopColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color PressedTextColor => pressedTextColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color FocusedBorderColor => focusedBorderColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color FocusedBackgroundStartColor => focusedBackgroundStartColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color FocusedBackgroundStopColor => focusedBackgroundStopColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color FocusedTextColor => focusedTextColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color DisabledBorderColor => disabledBorderColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color DisabledBackgroundStartColor => disabledBackgroundStartColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color DisabledBackgroundStopColor => disabledBackgroundStopColor;



        /********************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /********************************************************************/
        public Color DisabledTextColor => disabledTextColor;
    }
}
