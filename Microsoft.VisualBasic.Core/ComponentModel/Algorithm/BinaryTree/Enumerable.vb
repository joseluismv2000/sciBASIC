﻿#Region "Microsoft.VisualBasic::3587265eea5511f063323d0233fa5c6c, Microsoft.VisualBasic.Core\ComponentModel\Algorithm\BinaryTree\Enumerable.vb"

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

    '     Module Enumerable
    ' 
    '         Function: PopulateNodes, PopulateSequence, (+2 Overloads) Values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    Public Module Enumerable

        ''' <summary>
        ''' Populate an ASC sortted sequence from this binary tree 
        ''' 
        ''' ```
        ''' left -> me -> right
        ''' ```
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateSequence(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of Map(Of K, V))
            Return tree.PopulateNodes.Select(Function(n) New Map(Of K, V)(n.Key, n.Value))
        End Function

        <Extension>
        Public Iterator Function PopulateNodes(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of BinaryTree(Of K, V))
            If Not tree.Left Is Nothing Then
                For Each node In tree.Left.PopulateNodes
                    Yield node
                Next
            End If

            Yield tree

            If Not tree.Right Is Nothing Then
                For Each node In tree.Right.PopulateNodes
                    Yield node
                Next
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of V)
            Return tree.PopulateNodes.Values
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of K, V)(nodes As IEnumerable(Of BinaryTree(Of K, V))) As IEnumerable(Of V)
            Return nodes.Select(Function(n) n.Value)
        End Function
    End Module
End Namespace
