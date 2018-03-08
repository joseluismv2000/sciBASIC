﻿#Region "Microsoft.VisualBasic::0a86da51515c0329f02f968397ae3213, gr\Microsoft.VisualBasic.Imaging\SVG\XML\g.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Interface ICanvas
    ' 
    '         Properties: circles, gs, lines, path, polygon
    '                     polyline, rect, texts, title, transform
    ' 
    '     Class g
    ' 
    '         Properties: circles, gs, lines, path, polygon
    '                     polyline, rect, texts, title, transform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace SVG.XML

    ''' <summary>
    ''' SVG之中的画布对象，<see cref="SVGXml"/>和<see cref="g"/>都属于这种类型
    ''' </summary>
    Public Interface ICanvas
        Property transform As String
        Property texts As text()
        Property gs As g()
        Property path As path()
        Property rect As rect()
        Property polygon As polygon()
        Property polyline As polyline()
        Property lines As line()
        Property circles As circle()
        Property title As String
    End Interface

    ''' <summary>
    ''' SVG graphics unit
    ''' </summary>
    Public Class g : Inherits node
        Implements ICanvas

        <XmlAttribute> Public Property transform As String Implements ICanvas.transform
        <XmlElement("g")> Public Property gs As g() Implements ICanvas.gs
        <XmlElement> Public Property path As path() Implements ICanvas.path
        <XmlElement> Public Property rect As rect() Implements ICanvas.rect
        <XmlElement> Public Property polygon As polygon() Implements ICanvas.polygon
        <XmlElement("line")> Public Property lines As line() Implements ICanvas.lines
        <XmlElement("circle")> Public Property circles As circle() Implements ICanvas.circles
        <XmlElement> Public Property title As String Implements ICanvas.title
        <XmlElement> Public Property polyline As polyline() Implements ICanvas.polyline
        <XmlElement("text")> Public Property texts As text() Implements ICanvas.texts
    End Class
End Namespace
