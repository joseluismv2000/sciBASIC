﻿#Region "Microsoft.VisualBasic::927dce863d63dc8e9805a8a350b00061, ..\sciBASIC#\www\Microsoft.VisualBasic.Webservices.Bing\Academic.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

''' <summary>
''' Bing Academic web API for VisualBasic
''' </summary>
<Package("Bing.Academic",
         Url:="http://cn.bing.com/academic/?FORM=Z9LH2",
         Description:="",
         Category:=APICategories.UtilityTools,
         Publisher:="")>
Public Module Academic

    ' https://cn.bing.com/academic/search?q=Danio+rerio&go=%E6%90%9C%E7%B4%A2&qs=ds&form=QBRE

    Const refer$ = "https://cn.bing.com/academic/?FORM=Z9LH2"

    Public Function Search(term As String) As NamedValue(Of String)()
        Dim url$ = $"https://cn.bing.com/academic/search?q={term.UrlEncode}&go=Search&qs=ds&form=QBRE"
        Dim html$ = url.GET(headers:=New Dictionary(Of String, String) From {{NameOf(refer), refer}})

        html = html _
            .RemovesJavaScript _
            .RemovesCSSstyles _
            .RemovesImageLinks _
            .RemovesHtmlHead _
            .RemovesFooter
        html = Strings.Split(html, "<ol id=""b_results""").Last

        Dim list As NamedValue(Of String)() = r _
            .Matches(html, "<li class[=]""aca_algo"">.+?</li>", RegexICSng) _
            .EachValue(AddressOf StripListItem) _
            .ToArray

        Return list
    End Function

    Private Function StripListItem(text As String) As NamedValue(Of String)
        Dim title = text.GetBetween("<h2", "</h2>")
        Dim ResultUrl = title.href
        Dim description = text _
            .GetBetween("<div class=""caption_abstract"">", "</div>") _
            .GetBetween("<p>", "</p>") _
            .StripHTMLTags

        ResultUrl = "https://cn.bing.com/" & ResultUrl.Trim("/"c).Replace("&amp;", "&")
        title = title _
            .RemovesHtmlStrong _
            .GetValue _
            .StripHTMLTags

        Return New NamedValue(Of String) With {
            .Name = title,
            .Value = ResultUrl,
            .Description = description
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetDetails(info As NamedValue(Of String))
        Return GetDetails(info.Value)
    End Function

    Public Function GetDetails(url As String)

    End Function
End Module

Public Structure ArticleInfo
    Dim Title As String
    Dim Authors As String()
    Dim Abstract As String
    Dim PubDate As Date
    Dim Journal As String
    Dim DOI As String
    ''' <summary>
    ''' 按照年计数的被引用量
    ''' </summary>
    Dim CitesCount As NamedValue(Of Integer)()
    Dim Pages As String
    ''' <summary>
    ''' 卷号
    ''' </summary>
    Dim Volume As String
    ''' <summary>
    ''' 期号
    ''' </summary>
    Dim Issue As String
    ''' <summary>
    ''' 有效的原文来源地址url
    ''' </summary>
    Dim source As String()

    Public Overrides Function ToString() As String
        Return Title
    End Function
End Structure
