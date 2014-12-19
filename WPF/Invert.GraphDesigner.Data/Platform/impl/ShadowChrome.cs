using System.Windows.Media;

namespace DiagramDesigner.Platform
{
    class ShadowChrome
    {
        // *** Fields ***

        private static SolidColorBrush backgroundBrush;
        private static LinearGradientBrush rightBrush;
        private static LinearGradientBrush bottomBrush;
        private static RadialGradientBrush bottomRightBrush;
        private static RadialGradientBrush topRightBrush;
        private static RadialGradientBrush bottomLeftBrush;

        // *** Constructors ***

        static ShadowChrome()
        {
          
            CreateBrushes();
        }


        // *** Overriden base methods ***


        public static void DrawShadow(DrawingContext drawingContext, double shadowSize,float width, float height)
        {

            if (shadowSize <= 0 || width < shadowSize * 2 || height < shadowSize * 2)
                return;


            // Draw the background (this may show through rounded corners of the child object)
            var backgroundRect = new System.Windows.Rect(shadowSize, shadowSize, width - shadowSize, height - shadowSize);
            drawingContext.DrawRectangle(backgroundBrush, null, backgroundRect);
            // Now draw the shadow gradients
            var topRightRect = new System.Windows.Rect(width, shadowSize, shadowSize, shadowSize);
            drawingContext.DrawRectangle(topRightBrush, null, topRightRect);
            var rightRect = new System.Windows.Rect(width, shadowSize * 2, shadowSize,height - shadowSize * 2);
            drawingContext.DrawRectangle(rightBrush, null, rightRect);
            var bottomRightRect = new System.Windows.Rect(width, height, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomRightBrush, null, bottomRightRect);
            var bottomRect = new System.Windows.Rect(shadowSize * 2, height, width - shadowSize * 2, shadowSize);
            drawingContext.DrawRectangle(bottomBrush, null, bottomRect);
            var bottomLeftRect = new System.Windows.Rect(shadowSize, height, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomLeftBrush, null, bottomLeftRect);
        }


        // *** Private static methods ***


        private static void CreateBrushes()
        {
           
        }
    }
}