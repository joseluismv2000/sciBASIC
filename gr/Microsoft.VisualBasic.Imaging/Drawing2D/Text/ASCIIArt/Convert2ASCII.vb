Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Image2ASCII

    ''' <summary>
    ''' Program that converts images to ASCII art images
    ''' 
    ''' > https://github.com/juangallostra/Image2ASCII
    ''' </summary>
    Public Module HelperMethods

        ''' <summary>
        ''' Image input <paramref name="monoImage"/> should be processed by <see cref="Binarization"/> or <see cref="Grayscale"/>, without colors.
        ''' </summary>
        ''' <param name="monoImage"></param>
        ''' <param name="characters"></param>
        ''' <returns></returns>
        <Extension> Public Function Convert2ASCII(monoImage As Image, characters As List(Of WeightedChar)) As String
            '
            '             * ALGORITHM:
            '             * 
            '             *  1- Get target Image size (w=Width,h=Height)
            '             *  2- Create Result Image with white background and size W = w*character_image_width
            '             *                                                        H = h*character_image_height
            '             *  3- Create empty string to hold the text   
            '             *  
            '             *  4- for (int j=0;j=target_Image_Height;j++) --> ALL ROWS 
            '             *       5- Create row text string
            '             *       for (int i=0;i=target_Image_Width;i++) --> ALL COLUMNS
            '             *          6- Get target pixel weight
            '             *          7- Get closest weight from character list
            '             *          8- Paste character image in position w = i*character_image_width
            '             *                                               h = j*character_image_height
            '             *            ¡¡ Be careful with coordinate system when placing Images !!
            '             *          9- Add (string)character to row text string
            '             *       10- Add row text string to text holding string
            '             *  11 - return resulting Image & Text
            '             
            Dim out As New StringBuilder
            Dim BlackAndWhite As Bitmap = DirectCast(monoImage, Bitmap)

            For j As Integer = 0 To monoImage.Height - 1
                ' ROW
                Dim RowText As New List(Of String)() From {}

                For i As Integer = 0 To monoImage.Width - 1
                    ' COLUMN
                    Dim pixel As Color = BlackAndWhite.GetPixel(i, j)
                    Dim targetvalue As Double = (pixel.R + pixel.G + pixel.B) \ 3
                    Dim closestchar As WeightedChar = characters.Where(Function(t) Math.Abs(t.Weight - targetvalue) = characters.Min(Function(e) Math.Abs(e.Weight - targetvalue))).FirstOrDefault()
                    RowText.Add(closestchar.Character)
                Next

                out.AppendLine(RowText.JoinBy(""))
            Next

            Return out.ToString
        End Function

        <Extension> Public Function DrawText(text$, textColor As Color, backColor As Color, WidthAndHeight As SizeF) As Image
            ' Get char width for insertion point calculation purposes
            Dim dummy_img As Image = New Bitmap(1, 1)
            Dim dummy_drawing As Graphics = Graphics.FromImage(dummy_img)
            Dim textSize As SizeF = dummy_drawing.MeasureString(text, SystemFonts.DefaultFont)
            dummy_img.Dispose()
            dummy_drawing.Dispose()

            ' Create a dummy bitmap just to get a graphics object
            Dim img As Image = New Bitmap(1, 1)
            Dim drawing As Graphics = Graphics.FromImage(img)

            ' Free up resources taken by the dummy image and old graphics object
            img.Dispose()
            drawing.Dispose()

            ' Create a new image of the right size
            img = New Bitmap(CInt(Math.Truncate(WidthAndHeight.Width)), CInt(Math.Truncate(WidthAndHeight.Height)))
            ' Get a graphics object
            drawing = Graphics.FromImage(img)

            ' Paint the background
            drawing.Clear(backColor)

            ' Create a brush for the text
            Dim textBrush As Brush = New SolidBrush(textColor)

            drawing.DrawString(text, SystemFonts.DefaultFont, textBrush, (WidthAndHeight.Width - textSize.Width) / 2, 0)
            ' El punto de inserción del carácter se puede afinar más (Trial & Error)
            drawing.Save()

            textBrush.Dispose()
            drawing.Dispose()

            Return img
        End Function
    End Module
End Namespace

