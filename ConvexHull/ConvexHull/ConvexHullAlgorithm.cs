﻿using ConvexHull.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvexHull
{
    class ConvexHullAlgorithm
    {
        static void printS(List<PointF> S)
        {
            System.IO.File.WriteAllText("D:/points.txt", " ");
            for (int i = 0; i < S.Count; i++)
            {
                //Console.WriteLine(S[i].X + ", " + S[i].Y);
                System.IO.File.AppendAllText("D:/points.txt", S[i].X + ", " + S[i].Y + Environment.NewLine);

            }
        }

        static public void Compute(List<PointF> Points)
        {
            Points.Sort(delegate(PointF a, PointF b) { return a.X.CompareTo(b.X); });

            printS(Points);

            List<HullPoint> SS = new List<HullPoint>();

            for (int i = 0; i < Points.Count; i++)
            {
                SS.Add(new HullPoint(Points[i].X, Points[i].Y, i));
            }

            HullPoint start = ComputeCovexHull(SS, 0, Points.Count - 1).FirstOrDefault();
            HullPoint x = start;

            List<PointF> newPoints = new List<PointF>();
            do
            {
                newPoints.Add(new PointF(x.X, x.Y));
                x = x.next;
            } while (x != start);

        }

        // Case 1 and 4
        public static bool isTangent(HullPoint a, HullPoint b, List<HullPoint> U, int modifier)
        {
            for (int i = 0; i < U.Count; i++)
            {
                // Console.WriteLine("With : " + U[i].index + " " + Point.getScalarGz(a, b, U[i]) * direction);

                double x = Vector.getCrossProductZ(a, b, U[i]);

                if (modifier == 1 && x > 0)
                    return false;

                if (modifier == 2 && x < 0)
                    return false;

                if (modifier == 3 && x > 0)
                    return false;

                if (modifier == 4 && x < 0)
                    return false;
            }

            return true;

        }


        static public HullPoint findTangent(HullPoint a, HullPoint b, List<HullPoint> U, int direction, int modifier)
        {
            while (true)
            {
                if (isTangent(a, b, U, modifier)) return b;

                Console.WriteLine(b.index);

                if (direction == 1)
                    b = b.next;
                else
                    b = b.prev;
            }
        }

        // A is the left Convex Hull B is the right Convex Hull
        static public List<HullPoint> combine(List<HullPoint> A, List<HullPoint> B)
        {
            bool debug = true;

            // The rightmost point in A and the leftmost point in B are origins for the tangent search
            HullPoint rightA = A.Max();
            HullPoint leftB = B.Min();

            if (debug)
            {
                Console.WriteLine();
                Console.Write("\n\n## In A is : \n");
                foreach (HullPoint point in A)
                    Console.WriteLine(point.index + " " + point.X + ", " + point.Y);
                Console.Write("\n## In B is : \n");
                foreach (HullPoint point in B)
                    Console.WriteLine(point.index + " " + point.X + ", " + point.Y);
                Console.WriteLine();
            }

            // a and b are temporary variables for finding the upper tangent
            HullPoint a = rightA, b = leftB;
            while (true)
            {
                // Clockwise, Case 1
                b = findTangent(a, b, B, 1, 1);

                // Case 4
                if (isTangent(b, a, A, 4)) break;

                // Counterclockwise, Case 4
                a = findTangent(b, a, A, -1, 4);

                // Case 1
                if (isTangent(a, b, B, 1)) break;
            }
            HullPoint topA = a, topB = b;
            if (debug) Console.WriteLine("\n***Upper tangent is : " + a.index + " " + b.index);

            // a and b are temporary variables for finding the bottom tangent
            a = rightA; b = leftB;
            while (true)
            {
                // Counterclockwise, Case 2
                b = findTangent(a, b, B, -1, 2);

                // Case 3
                if (isTangent(b, a, A, 3)) break;

                // Clockwise, Case 3
                a = findTangent(b, a, A, 1, 3);

                // Case 2
                if (isTangent(a, b, B, 2)) break;
            }
            HullPoint bottomA = a, bottomB = b;
            if (debug) Console.WriteLine("\n*** Lower tangent is : " + a.index + " " + b.index);


            // "Stiching" up the top and bottom of the two Convex Hulls together
            topA.next = topB; topB.prev = topA;

            bottomA.prev = bottomB; bottomB.next = bottomA;

            // The Union of both convex hull can be found by starting at any of the Tangent points and walking untill the same point is reached again
            List<HullPoint> U = new List<HullPoint>();

            HullPoint x = a;
            if (debug) Console.Write("\n\nThe run is : \n");
            do
            {
                U.Add(x);
                if (debug) Console.Write(x.X + ", " + x.Y + Environment.NewLine);
                x = x.next;
            } while (x != a);

            return U;
        }


        static private List<HullPoint> ComputeCovexHull(List<HullPoint> S, int l, int r)
        {
            // Base case with 1 point is a Simple Convex Hull
            if (r - l + 1 == 1)
            {
                List<HullPoint> C = new List<HullPoint>();
                C.Add(S[l]);
                C[0].next = C[0];
                C[0].prev = C[0];
                return C;
            }

            // Base case with 2 points is a Convex Hull with both of them
            if (r - l + 1 == 2)
            {
                List<HullPoint> C = new List<HullPoint>();

                C.Add(S[l]); C.Add(S[r]);

                // Important they have to be linked together
                C[0].next = C[1]; C[0].prev = C[1];

                C[1].next = C[0]; C[1].prev = C[0];

                return C;
            }

            int mid = (l + r) / 2;

            List<HullPoint> A = ComputeCovexHull(S, l, mid);
            List<HullPoint> B = ComputeCovexHull(S, mid + 1, r);

            return combine(A, B);
        }


       


    }
}