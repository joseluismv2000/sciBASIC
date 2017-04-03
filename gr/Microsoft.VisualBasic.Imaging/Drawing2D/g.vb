﻿#Region "Microsoft.VisualBasic::cdcd09da8a8ca408542f1c6463ef991b, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\g.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace Drawing2D

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g">GDI+设备</param>
    ''' <param name="grct">绘图区域的大小</param>
    Public Delegate Sub IPlot(ByRef g As IGraphics, grct As GraphicsRegion)

    ''' <summary>
    ''' Data plots graphics engine common abstract.
    ''' </summary>
    Public Module g

        Public Const DefaultPadding$ = "padding:100px 100px 100px 100px;"

        ''' <summary>
        ''' 与<see cref="DefaultPadding"/>相比而言，这个padding的值在坐标轴Axis的label的绘制上空间更加大
        ''' </summary>
        Public Const DefaultLargerPadding$ = "padding:100px 100px 150px 150px;"
        Public Const ZeroPadding$ = "padding: 0px 0px 0px 0px;"

        ''' <summary>
        ''' Data plots graphics engine. Default: <paramref name="size"/>:=(4300, 2000), <paramref name="padding"/>:=(100,100,100,100)
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="padding"></param>
        ''' <param name="bg">颜色值或者图片资源文件的url或者文件路径</param>
        ''' <param name="plotAPI"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GraphicsPlots(ByRef size As Size, ByRef padding As Padding, bg$, plotAPI As IPlot, Optional driver As Drivers = Drivers.GDI) As GraphicsData
            Dim image As GraphicsData

            If size.IsEmpty Then
                size = New Size(3600, 2000)
            End If
            If padding.IsEmpty Then
                padding = New Padding(100)
            End If

            If driver = Drivers.SVG Then
                Dim svg As New GraphicsSVG
                Call svg.Clear(bg.TranslateColor)
                Call plotAPI(svg, New GraphicsRegion With {
                       .Size = size,
                       .Padding = padding
                  })

                image = New SVGData(svg, size)
            Else
                ' using gdi+ graphics driver
                ' 在这里使用透明色进行填充，防止当bg参数为透明参数的时候被CreateGDIDevice默认填充为白色
                Using g As Graphics2D = size.CreateGDIDevice(Color.Transparent)
                    Dim rect As New Rectangle(New Point, size)

                    With g.Graphics

                        Call .FillBg(bg$, rect)

                        .CompositingQuality = CompositingQuality.HighQuality
                        .CompositingMode = CompositingMode.SourceOver
                        .InterpolationMode = InterpolationMode.HighQualityBicubic
                        .PixelOffsetMode = PixelOffsetMode.HighQuality
                        .SmoothingMode = SmoothingMode.HighQuality
                        .TextRenderingHint = TextRenderingHint.ClearTypeGridFit

                    End With

                    Call plotAPI(g, New GraphicsRegion With {
                         .Size = size,
                         .Padding = padding
                    })

                    image = New ImageData(g.ImageResource, size)
                End Using
            End If

            Return image
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="bg$">
        ''' 1. 可能为颜色表达式
        ''' 2. 可能为图片的路径
        ''' 3. 可能为base64图片字符串
        ''' </param>
        <Extension>
        Public Sub FillBg(ByRef g As Graphics, bg$, rect As Rectangle)
            Dim bgColor As Color = bg.ToColor(onFailure:=Nothing)

            If Not bgColor.IsEmpty Then
                Call g.FillRectangle(New SolidBrush(bgColor), rect)
            Else
                Dim res As Drawing.Image

                If bg.FileExists Then
                    res = LoadImage(path:=bg$)
                Else
                    res = Base64Codec.GetImage(bg$)
                End If

                Call g.DrawImage(res, rect)
            End If
        End Sub

        ''' <summary>
        ''' Data plots graphics engine.
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="bg"></param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GraphicsPlots(plot As Action(Of IGraphics), ByRef size As Size, ByRef padding As Padding, bg$) As GraphicsData
            Return GraphicsPlots(size, padding, bg, Sub(ByRef g, rect) Call plot(g))
        End Function

        Public Function Allocate(Optional size As Size = Nothing, Optional padding$ = DefaultPadding, Optional bg$ = "white") As InternalCanvas
            Return New InternalCanvas With {
                .size = size,
                .bg = bg,
                .padding = padding
            }
        End Function

        ''' <summary>
        ''' 可以借助这个画布对象创建多图层的绘图操作
        ''' </summary>
        Public Class InternalCanvas

            Dim plots As New List(Of IPlot)

            Public Property size As Size
            Public Property padding As Padding
            Public Property bg As String

            Public Function InvokePlot() As GraphicsData
                Return GraphicsPlots(
                    size, padding, bg,
                    Sub(ByRef g, rect)

                        For Each plot As IPlot In plots
                            Call plot(g, rect)
                        Next
                    End Sub)
            End Function

            Public Shared Operator +(g As InternalCanvas, plot As IPlot) As InternalCanvas
                g.plots += plot
                Return g
            End Operator

            Public Shared Operator +(g As InternalCanvas, plot As IPlot()) As InternalCanvas
                g.plots += plot
                Return g
            End Operator

            Public Shared Narrowing Operator CType(g As InternalCanvas) As GraphicsData
                Return g.InvokePlot
            End Operator

            ''' <summary>
            ''' canvas invoke this plot.
            ''' </summary>
            ''' <param name="g"></param>
            ''' <param name="plot"></param>
            ''' <returns></returns>
            Public Shared Operator <=(g As InternalCanvas, plot As IPlot) As GraphicsData
                Dim size As Size = g.size
                Dim margin = g.padding
                Dim bg As String = g.bg

                Return GraphicsPlots(size, margin, bg, plot)
            End Operator

            Public Shared Operator >=(g As InternalCanvas, plot As IPlot) As GraphicsData
                Throw New NotSupportedException
            End Operator
        End Class
    End Module
End Namespace
