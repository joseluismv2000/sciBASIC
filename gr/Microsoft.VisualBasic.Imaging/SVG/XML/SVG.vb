﻿#Region "Microsoft.VisualBasic::fdd82b8b50c9c10a3f46b54ac166d9bf, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SVG.vb"

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

'     Class SVGXml
' 
'         Properties: circles, defs, enable_background, gs, height
'                     id, images, lines, overflow, path
'                     polygon, polyline, rect, space, style
'                     texts, title, transform, version, viewBox
'                     width, WriterComment
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: GetSVGXml, (+2 Overloads) SaveAsXml, TryLoad
' 
'         Sub: SetSize
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Text

Namespace SVG.XML

    ' 2018-1-22
    ' XmlType / XmlRoot是不一样的？
    ' 如果要修改根节点的xmlns的话，则必须要使用XmlRoot来进行修饰

    ''' <summary>
    ''' The svg vector graphics in Xml document format.
    ''' </summary>
    <XmlRoot("svg", [Namespace]:=SVGWriter.Xmlns)> Public Class SVGXml
        Implements ISaveHandle
        Implements ICanvas

#Region "xml root property"
        <XmlNamespaceDeclarations()>
        Public xmlns As New XmlSerializerNamespaces
        <XmlIgnore>
        Public XmlComment$

        Public Sub New()
            xmlns.Add("xlink", SVGWriter.Xlink)
        End Sub

        Sub New(width%, height%)
            Call Me.New
            Call Me.Size(New Size(width, height))
        End Sub

        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property version As String
        <XmlAttribute> Public Property viewBox As String()
        <XmlAttribute> Public Property overflow As String

        <XmlAttribute("enable-background")>
        Public Property enable_background As String

        <XmlAttribute("space", [Namespace]:=SVGWriter.Xmlns)>
        Public Property space As String
#End Region

        ''' <summary>
        ''' Style definition of the xml node in this svg document. 
        ''' you can define the style by using css and set the class 
        ''' attribute for the specific node to controls the 
        ''' visualize style.
        ''' </summary>
        ''' <returns></returns>
        Public Property defs As CSSStyles
        ''' <summary>
        ''' SVG对象也会在这里面定义CSS
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("style")> Public Shadows Property style As XmlMeta.CSS
        <XmlElement("image")> Public Property images As Image() Implements ICanvas.images

        Const declare$ = "SVG document was created by sciBASIC svg image driver:"

        ''' <summary>
        ''' Xml comment for <see cref="Layers"/>
        ''' </summary>
        ''' <returns></returns>
        <XmlAnyElement("gComment")>
        Public Property WriterComment As XmlComment
            Get
                Dim [rem] As New StringBuilder
                Dim indent As New String(" "c, 6)

                Call [rem].AppendLine _
                          .Append(indent) _
                          .AppendLine([declare]) _
                          .AppendLine _
                          .Append(indent & New String(" "c, 3)) _
                          .AppendLine("visit: " & LICENSE.githubURL)

                If Not XmlComment.StringEmpty Then
                    For Each line As String In XmlComment.lTokens
                        [rem].AppendLine _
                             .Append(indent) _
                             .Append(line)
                    Next
                End If

                [rem].AppendLine _
                     .Append("  ")

                Return New XmlDocument().CreateComment([rem].ToString)
            End Get
            Set
            End Set
        End Property

        Dim _layers As List(Of g)

        ''' <summary>
        ''' Graphic layers
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' SVG无法通过调整``z-index``来设置图层位置，在这里需要使用一个List动态列表
        ''' 调整呈现SVG里面的图层元素
        ''' </remarks>
        <XmlElement("g")>
        Public Property Layers As g() Implements ICanvas.Layers
            Get
                Return _layers.ToArray
            End Get
            Set(value As g())
                _layers = New List(Of g)(value)
            End Set
        End Property

        <XmlAttribute> Public Property transform As String Implements ICanvas.transform
        <XmlElement("text")> Public Property texts As text() Implements ICanvas.texts
        <XmlElement> Public Property path As path() Implements ICanvas.path
        <XmlElement> Public Property rect As rect() Implements ICanvas.rect
        <XmlElement> Public Property polygon As polygon() Implements ICanvas.polygon
        <XmlElement("line")> Public Property lines As line() Implements ICanvas.lines
        <XmlElement("circle")> Public Property circles As circle() Implements ICanvas.circles
        <XmlElement> Public Property title As String Implements ICanvas.title
        <XmlElement> Public Property polyline As polyline() Implements ICanvas.polyline

        Public Function Size(sz As Size) As SVGXml
            width = sz.Width & "px"
            height = sz.Height & "px"
            Return Me
        End Function

        Public Function AddLayer(layer As g) As SVGXml
            _layers.Add(item:=layer)
            Return Me
        End Function

        ''' <summary>
        ''' Load SVG object from a specific xml file path or xml file text content.
        ''' </summary>
        ''' <param name="xml"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function TryLoad(xml As String) As SVGXml
            Return xml.SolveStream.LoadFromXml(Of SVGXml)(throwEx:=True)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function SaveAsXml(Optional path$ = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            Return GetSVGXml.SaveTo(path, encoding)
        End Function

        ''' <summary>
        ''' 将当前的这个SVG对象序列化为XML字符串文本
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSVGXml() As String
            Return GetXml
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveAsXml(Optional path$ = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(path, encoding.CodePage)
        End Function
    End Class
End Namespace
