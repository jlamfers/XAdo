using System.Collections.Generic;
using System.Linq;
using XAdo.Core;

namespace XAdo.DbSchema
{
   /// <summary>
   /// https://en.wikipedia.org/wiki/Topological_sorting
   /// 
   /// implementation base on: 
   /// http://tawani.blogspot.nl/2009/02/topological-sorting-and-cyclic.html
   /// 
   /// to be used for determining delete order/insert order
   /// NOTE: I tried other simpler implementations, unfortunately all of these did not work under all circumstances
   /// 
   /// </summary>
   public class TopologicalSort
   {

      public static IEnumerable<T> Sort<T>(IList<TopologicalSortNode<T>> nodes)
      {
         var sorter = GetSorter(nodes);
         var index = sorter.Sort();
         return index.Select(i => nodes[i].Item);
      }

      private readonly int[] 
         _vertices; // list of vertices

      private readonly int[,] 
         _matrix; // adjacency matrix

      private int 
         _actualVertsCount; // current number of vertices

      private readonly int[] 
         _sortedArray;

      private TopologicalSort(int size)
      {
         _vertices = new int[size];
         _matrix = new int[size, size];
         _actualVertsCount = 0;
         for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
               _matrix[i, j] = 0;
         _sortedArray = new int[size]; // sorted vert labels
      }

      private static TopologicalSort GetSorter<T>(IList<TopologicalSortNode<T>> nodes)
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
               foreach (var edge in nodes[i].DependsOn)
               {
                  sorter.AddEdge(i, lookup[edge]);
               }
            }
         }
         return sorter;

      }

      private int AddVertex(int vertex)
      {
         _vertices[_actualVertsCount++] = vertex;
         return _actualVertsCount - 1;
      }

      private void AddEdge(int start, int end)
      {
         _matrix[start, end] = 1;
      }

      private int[] Sort() // toplogical sort
      {
         var count = _sortedArray.Length;

         while (_actualVertsCount > 0) // while vertices remain,
         {
            // get a vertex with no successors, or -1
            var currentVertex = NoSuccessors();
            if (currentVertex == -1) // must be a cycle    
            {
               throw new XAdoException("The graph has at least one cycle. Topological sorting is impossible.");
            }

            // insert vertex label in sorted array 
            _sortedArray[count - _actualVertsCount] = _vertices[currentVertex];

            DeleteVertex(currentVertex); // delete vertex
         }

         // vertices all gone; return sortedArray
        return _sortedArray;
      }

      // returns vert with no successors (or -1 if no such verts)
      private int NoSuccessors()
      {
         for (int row = 0; row < _actualVertsCount; row++)
         {
            bool isEdge = false; // edge from row to column in adjMat
            for (int col = 0; col < _actualVertsCount; col++)
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

      private void DeleteVertex(int vertex)
      {
         // if not last vertex, delete from vertexList
         if (vertex != _actualVertsCount - 1)
         {
            for (var j = vertex; j < _actualVertsCount - 1; j++)
               _vertices[j] = _vertices[j + 1];

            for (var row = vertex; row < _actualVertsCount - 1; row++)
               MoveRowUp(row, _actualVertsCount);

            for (var col = vertex; col < _actualVertsCount - 1; col++)
               MoveColLeft(col, _actualVertsCount - 1);
         }
         _actualVertsCount--; // one less vertex
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
