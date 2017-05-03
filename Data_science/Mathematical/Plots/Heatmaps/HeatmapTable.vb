﻿#Region "Microsoft.VisualBasic::49988d2e7b07844d474b04de6f93ec57, ..\sciBASIC#\Data_science\Mathematical\Plots\Heatmaps\HeatmapTable.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module HeatmapTable

    ''' <summary>
    ''' 只能够用来表示两两变量之间的相关度
    ''' </summary>
    ''' <param name="triangularStyle">
    ''' 是否下三角部分显示圆，默认是本三角样式
    ''' 圆的半径大小用来表示相关度的绝对值，颜色则是和普通的heatmap一样用来表示相关度的大小和方向
    ''' </param>
    ''' <returns></returns>
    Public Function Plot(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double))),
                         Optional mapLevels% = 20,
                         Optional mapName$ = ColorMap.PatternJet,
                         Optional size As Size = Nothing,
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional triangularStyle As Boolean = True,
                         Optional fontStyle$ = CSSFont.Win10Normal,
                         Optional legendTitle$ = "Heatmap Color Legend",
                         Optional legendFont As Font = Nothing,
                         Optional range As DoubleRange = Nothing,
                         Optional mainTitle$ = "heatmap",
                         Optional titleFont As Font = Nothing,
                         Optional drawGrid As Boolean = False,
                         Optional drawValueLabel As Boolean = True,
                         Optional valuelabelFontCSS$ = CSSFont.PlotLabelNormal) As GraphicsData

        Dim margin As Padding = padding
        Dim valuelabelFont As Font = CSSFont.TryParse(valuelabelFontCSS)
        Dim array = data.ToArray
        Dim min#, max#
        Dim plotInternal =
            Sub(g As IGraphics, region As GraphicsRegion, left As Value(Of Single), font As Font, dw As Single, levels As Dictionary(Of Double, Integer), top As Value(Of Single), colors As Color())
                ' 在绘制上三角的时候假设每一个对象的keys的顺序都是相同的
                Dim keys$() = array(Scan0).Value.Keys.ToArray
                Dim blockSize As New SizeF(dw, dw)  ' 每一个方格的大小

                For Each x As SeqValue(Of NamedValue(Of Dictionary(Of String, Double))) In array.SeqIterator(offset:=1)  ' 在这里绘制具体的矩阵

                    Dim i% = 1

                    For Each key$ In keys
                        Dim c# = (+x).Value(key)
                        Dim rect As New RectangleF(New PointF(left, top), blockSize)
                        Dim labelbrush As SolidBrush = Nothing

                        If triangularStyle AndAlso i > x.i Then ' 上三角部分不绘制任何图形
                            ' labelbrush = Brushes.Black
                        Else
                            Dim level% = levels(c#)  '  得到等级
                            Dim color As Color = colors(   ' 得到当前的方格的颜色
                                If(level% > colors.Length - 1,
                                colors.Length - 1,
                                level))
                            Dim b As New SolidBrush(color)
                            Dim r As Single = Math.Abs(c) * dw / 2 ' 计算出半径的大小
                            Dim d = dw / 2

                            r *= 2

                            If drawValueLabel Then
                                labelbrush = Brushes.White
                            End If

                            Call g.FillPie(b, rect.Left + d, rect.Top + d, r, r, 0, 360)
                        End If

                        If drawGrid Then
                            Call g.DrawRectangles(Pens.WhiteSmoke, {rect})
                        End If
                        If Not labelbrush Is Nothing Then
                            key = c.FormatNumeric(2)
                            Dim ksz As SizeF = g.MeasureString(key, valuelabelFont)
                            Dim kpos As New PointF(rect.Left + (rect.Width - ksz.Width) / 2, rect.Top + (rect.Height - ksz.Height) / 2)
                            Call g.DrawString(key, valuelabelFont, labelbrush, kpos)
                        End If

                        left.value += dw!
                        i += 1
                    Next

                    left.value = margin.Left
                    top.value += dw!

                    Dim sz As SizeF = g.MeasureString((+x).Name, font)
                    Dim y As Single = top.value - dw - (sz.Height - dw) / 2
                    Dim lx! = margin.Left - sz.Width - margin.Horizontal * 0.1

                    Call g.DrawString((+x).Name, font, Brushes.Black, New PointF(lx, y))
                Next
            End Sub

        If range Is Nothing Then
            range = New DoubleRange(
                array _
                .Select(Function(x) x.Value.Values) _
                .IteratesALL _
                .ToArray)
        End If

        With range
            min = .Min
            max = .Max
        End With

        Return Heatmap.__plotInterval(
            plotInternal, data.ToArray,,
            mapLevels, mapName,
            size, margin, bg,
            fontStyle, legendTitle,
            legendFont, min, max,
            mainTitle, titleFont)
    End Function
End Module
