/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Drawing;

using PurplePen.MapModel;
using PurplePen.Graphics2D;

namespace PurplePen
{
    // Mode for rotating an object.
    class RotateMode: BaseMode
    {
        Controller controller;
        //JU: Generic course object to rotate any oject type
        CourseObj courseObj;                    // object to modify.
        PointF rotationPoint;                   // center point for rotation calculation


        public RotateMode(Controller controller, CourseObj courseObj)
        {
            this.controller = controller;
            this.courseObj = (CourseObj) courseObj.Clone();

            // Get the rotation center point based on object type
            this.rotationPoint = GetRotationPoint(courseObj);
        }

        // Mouse cursor looks like a crosshair
        public override MousePointerShape GetMouseCursor(Pane pane, PointF location, float pixelSize)
        {
            if (pane == Pane.Map)
                return MousePointerShape.Cross;
            else
                return MousePointerShape.Arrow;
        }

        public override string StatusText
        {
            get
            {
                return StatusBarText.RotatingObject;
            }
        }

        public override IMapViewerHighlight[] GetHighlights(Pane pane)
        {
            if (pane != Pane.Map)
                return null;

            return new CourseObj[1] { courseObj };
        }

        public override DragAction LeftButtonDown(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane != Pane.Map)
                return DragAction.None;

            // Create the new corner
            RotateToAngle(location);
            controller.Rotate(GetOrientation(courseObj));
            controller.DefaultCommandMode();
            return DragAction.None;
        }

        public override void MouseMoved(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane != Pane.Map)
                return;

            RotateToAngle(location);
            displayUpdateNeeded = true;
        }

        // Change the orientation of the crossing point course object to the given angle in degrees.
        private void RotateToAngle(PointF point)
        {
            double angleInRadians = Math.Atan2(point.Y - rotationPoint.Y, point.X - rotationPoint.X);
            float angleInDegrees = (float) Geometry.RadiansToDegrees(angleInRadians);
            courseObj = (CourseObj) courseObj.Clone();
            //courseObj.ChangeOrientation(angleInDegrees);
            SetOrientation(courseObj, angleInDegrees);
        }

        // Get the rotation center point based on the object type
        private PointF GetRotationPoint(CourseObj obj)
        {
            // TextCourseObj, rotate around topLeft corner
            if (obj is TextCourseObj textObj)
            {
                //return textObj.GetHighlightBounds().Center();
                return textObj.topLeft;
            }

            // PointCourseObj like CrossingCourseObj and ForbiddenCourseObj, rotate around ref point
            if (obj is PointCourseObj pointObj)
            {
                return pointObj.location;
            }

            if (obj is RectCourseObj rectObj)
            {
                return rectObj.rect.Center();
            }

            // Default: return approximate center
            return obj.GetHighlightBounds().Center();
        }

        // Get current orientation from any rotatable object
        private float GetOrientation(CourseObj obj)
        {
            // TextCourseObj
            if (obj is TextCourseObj textObj)
            {
                return textObj.orientation;
            }

            // PointCourseObj like CrossingCourseObj and ForbiddenCourseObj
            if (obj is PointCourseObj pointObj)
            {
                return pointObj.orientation;
            }

            /*
            if (obj is RectCourseObj rectObj)
            {
                return rectObj.orientation;
            }
            */

            return 0F;
        }

        // Set orientation for any rotatable object using reflection
        private void SetOrientation(CourseObj obj, float angle)
        {
            // TextCourseObj
            if (obj is TextCourseObj textObj)
            {
                textObj.orientation = angle;
                return;
            }

            // PointCourseObj like CrossingCourseObj and ForbiddenCourseObj
            if (obj is PointCourseObj pointObj)
            {
                pointObj.orientation = angle;
                return;
            }

            /*
            if (obj is RectCourseObj rectObj)
            {
                rectObj.orientation = angle;
                return;
            }
            */
        }
    }
}
