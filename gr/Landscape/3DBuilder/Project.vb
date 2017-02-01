﻿#Region "Microsoft.VisualBasic::4ec67d8f2f794034c74413b94a2f4001, ..\sciBASIC#\gr\Landscape\3DBuilder\Project.vb"

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

Public Class Project

    ''' <summary>
    ''' ``/Metadata/thumbnail.png``
    ''' </summary>
    ''' <returns></returns>
    Public Property Thumbnail As Image
    Public Property model As XmlModel3D

    Public Shared Function FromZipDirectory(dir$) As Project
        Return New Project With {
            .Thumbnail = $"{dir}/Metadata/thumbnail.png".LoadImage,
            .model = IO.Load3DModel(dir & "/3D/3dmodel.model")
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="centraOffset"></param>
    ''' <returns></returns>
    Public Function GetSurfaces(Optional centraOffset As Boolean = False) As Drawing3D.Surface()
        If model Is Nothing Then
            Return {}
        Else
            Dim out As Drawing3D.Surface() = model.GetSurfaces.ToArray

            If centraOffset Then
                With out.Centra
                    out = .Offsets(out).ToArray
                End With
            End If

            Return out
        End If
    End Function
End Class

