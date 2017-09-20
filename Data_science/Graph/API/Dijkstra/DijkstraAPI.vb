﻿#Region "Microsoft.VisualBasic::5e729b32d3c52efa8e09fcf82ecb23af, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\FindPath\Dijkstra\DijkstraAPI.vb"

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
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Dijkstra

    <Package("Path.Find.Dijkstra",
                        Publisher:="Michael Demeersseman",
                        Category:=APICategories.UtilityTools,
                        Description:="Calculation of the shortest path between x points",
                        Url:="http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente")>
    Public Module DijkstraAPI

        <ExportAPI("Finder.Creates")>
        <Extension>
        Public Function CreatePathwayFinder(g As Graph, Optional undirected As Boolean = False) As DijkstraRouteFind
            Return New DijkstraRouteFind(g, undirected)
        End Function

        <ExportAPI("Find.Path.Shortest")>
        Public Function FindShortestPath(finder As DijkstraRouteFind, start$, ends$) As Route
            Return FindAllPath(finder, start, ends).FirstOrDefault
        End Function

        <ExportAPI("Network.Path.FindAll")>
        Public Function FindAllPath(finder As DijkstraRouteFind, start$, ends$) As Route()
            Dim route = finder.CalculateMinCost(start)
            Dim endIndex% = finder _
                .Locations _
                .Where(Function(v) v.Label = ends) _
                .FirstOrDefault _
               ?.ID
            Dim LQuery = (From rt In route.Values Where rt.ContainsNode(endIndex) Select rt Order By rt.Cost Ascending).ToArray
            Return LQuery
        End Function
    End Module
End Namespace
