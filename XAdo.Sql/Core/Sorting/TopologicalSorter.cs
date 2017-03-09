using System;
using System.Collections.Generic;
using System.Linq;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core.Sorting
{
   public class TopologicalSorter
   {
      private readonly int[] _vertices; // list of vertices
      private readonly int[,] _matrix; // adjacency matrix
      private int _numVerts; // current number of vertices
      private readonly int[] _sortedArray;

      public TopologicalSorter(int size)
      {
         _vertices = new int[size];
         _matrix = new int[size, size];
         _numVerts = 0;
         for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
               _matrix[i, j] = 0;
         _sortedArray = new int[size]; // sorted vert labels
      }

      public int AddVertex(int vertex)
      {
         _vertices[_numVerts++] = vertex;
         return _numVerts - 1;
      }

      public void AddEdge(int start, int end)
      {
         _matrix[start, end] = 1;
      }

      public int[] Sort() // toplogical sort
      {
         while (_numVerts > 0) // while vertices remain,
         {
            // get a vertex with no successors, or -1
            int currentVertex = NoSuccessors();
            if (currentVertex == -1) // must be a cycle                
               throw new Exception("Graph has cycles");

            // insert vertex label in sorted array (start at end)
            _sortedArray[_numVerts - 1] = _vertices[currentVertex];

            DeleteVertex(currentVertex); // delete vertex
         }

         // vertices all gone; return sortedArray
         return _sortedArray;
      }


      #region Privates

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

      #endregion

      public static IList<T> Sort<T>(IList<TopologicalNode<T>> nodes)
      {
         return GetTopologicalSortOrder(nodes).Select(index => nodes[index].Item).ToList();
      }

      public static IList<TablePartial> Sort(ISqlResource resource)
      {
         throw new NotImplementedException();
         var graph = new List<TopologicalNode<TablePartial>>();
         foreach (var table in resource.Tables)
         {
            var node = new TopologicalNode<TablePartial> {Item = table, DependsOn = new List<TablePartial>()};
            foreach (var join in resource.Joins)
            {
               foreach (var t in join.EquiJoinColumns)
               {
                  //TODO: find fkeys/pkeys
               }
            }
         }
         
      }

      private static IEnumerable<int> GetTopologicalSortOrder<T>(IList<TopologicalNode<T>> nodes)
      {
         var sorter = new TopologicalSorter(nodes.Count);
         var lookup = new Dictionary<T, int>();

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
                  sorter.AddEdge(i,lookup[nodes[i].DependsOn[j]]);
               }
            }
         }

         var result = sorter.Sort();
         return result;

      }
   }


}
