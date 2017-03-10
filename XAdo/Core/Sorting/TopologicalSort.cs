﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Core.Sorting
{
   /// <summary>
   /// https://en.wikipedia.org/wiki/Topological_sorting
   /// 
   /// implementation inspired by: 
   /// http://tawani.blogspot.nl/2009/02/topological-sorting-and-cyclic.html
   /// 
   /// In the field of computer science, a topological sort or topological ordering of a directed graph is a linear ordering of its vertices 
   /// such that for every directed edge uv from vertex u to vertex v, u comes before v in the ordering. For instance, the vertices of the graph 
   /// may represent tasks to be performed, and the edges may represent constraints that one task must be performed before another; 
   /// in this application, a topological ordering is just a valid sequence for the tasks. A topological ordering is possible if and only if the graph 
   /// has no directed cycles, that is, if it is a directed acyclic graph (DAG). Any DAG has at least one topological ordering, and algorithms 
   /// are known for constructing a topological ordering of any DAG in linear time.
   /// </summary>
   public class TopologicalSort
   {

      private readonly int[] _vertices; // list of vertices
      private readonly int[,] _matrix; // adjacency matrix
      private int _numVerts; // current number of vertices
      private readonly int[] _sortedArray;

      private TopologicalSort(int size)
      {
         _vertices = new int[size];
         _matrix = new int[size, size];
         _numVerts = 0;
         for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
               _matrix[i, j] = 0;
         _sortedArray = new int[size]; // sorted vert labels
      }

      public static IEnumerable<T> Sort<T>(IList<TopologicalSortNode<T>> nodes)
      {
         return GetTopologicalSortOrder(nodes).Select(index => nodes[index].Item);
      }

      private static IEnumerable<int> GetTopologicalSortOrder<T>(IList<TopologicalSortNode<T>> nodes)
      {
         var sorter = new TopologicalSort(nodes.Count);
         var lookup = new Dictionary<T, int>(nodes.Count);

         //add vertices
         for (var i = 0; i < nodes.Count; i++)
         {
            lookup[nodes[i].Item] = sorter.AddVertex(i);
         }

         //add edges
         for (var i = 0; i < nodes.Count; i++)
         {
            if (nodes[i].DependsOn != null)
            {
               for (var j = 0; j < nodes[i].DependsOn.Count; j++)
               {
                  sorter.AddEdge(i, lookup[nodes[i].DependsOn[j]]);
               }
            }
         }
         var result = sorter.Sort();
         return result;

      }

      private int AddVertex(int vertex)
      {
         _vertices[_numVerts++] = vertex;
         return _numVerts - 1;
      }

      private void AddEdge(int start, int end)
      {
         _matrix[start, end] = 1;
      }

      private int[] Sort() // toplogical sort
      {
         var count = _sortedArray.Length;

         while (_numVerts > 0) // while vertices remain,
         {
            // get a vertex with no successors, or -1
            var currentVertex = NoSuccessors();
            if (currentVertex == -1) // must be a cycle    
            {
               throw new XAdoException("The graph has at least one cycle. Topological sorting is impossible.");
            }

            // insert vertex label in sorted array 
            _sortedArray[count - _numVerts] = _vertices[currentVertex];

            DeleteVertex(currentVertex); // delete vertex
         }

         // vertices all gone; return sortedArray
        return _sortedArray;
      }

      // returns vert with no successors (or -1 if no such verts)
      private int NoSuccessors()
      {
         for (int row = 0; row < _numVerts; row++)
         {
            bool isEdge = false; // edge from row to column in adjMat
            for (int col = 0; col < _numVerts; col++)
            {
               if (_matrix[row, col] > 0) // if edge to another,
               {
                  isEdge = true;
                  break; // this vertex has a successor try another
               }
            }
            if (!isEdge) // if no edges, has no successors
               return row;
         }
         return -1; // no
      }

      private void DeleteVertex(int delVert)
      {
         // if not last vertex, delete from vertexList
         if (delVert != _numVerts - 1)
         {
            for (var j = delVert; j < _numVerts - 1; j++)
               _vertices[j] = _vertices[j + 1];

            for (var row = delVert; row < _numVerts - 1; row++)
               MoveRowUp(row, _numVerts);

            for (var col = delVert; col < _numVerts - 1; col++)
               MoveColLeft(col, _numVerts - 1);
         }
         _numVerts--; // one less vertex
      }

      private void MoveRowUp(int row, int length)
      {
         for (var col = 0; col < length; col++)
            _matrix[row, col] = _matrix[row + 1, col];
      }

      private void MoveColLeft(int col, int length)
      {
         for (var row = 0; row < length; row++)
            _matrix[row, col] = _matrix[row, col + 1];
      }

   }


}
