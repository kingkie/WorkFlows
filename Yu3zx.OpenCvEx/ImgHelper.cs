using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.OpenCvEx
{
    public class ImgHelper
    {
        //public static Bitmap MatchPicBySift(Bitmap imgSrc, Bitmap imgSub)
        //{
        //    using (Mat matSrc = imgSrc.ToMat())
        //    using (Mat matTo = imgSub.ToMat())
        //    using (Mat matSrcRet = new Mat())
        //    using (Mat matToRet = new Mat())
        //    {
        //        KeyPoint[] keyPointsSrc, keyPointsTo;
        //        using (var sift = OpenCvSharp.XFeatures2D.SIFT.Create())
        //        {
        //            sift.DetectAndCompute(matSrc, null, out keyPointsSrc, matSrcRet);
        //            sift.DetectAndCompute(matTo, null, out keyPointsTo, matToRet);
        //        }
        //        using (var bfMatcher = new OpenCvSharp.BFMatcher())
        //        {
        //            var matches = bfMatcher.KnnMatch(matSrcRet, matToRet, k: 2);
        //            var pointsSrc = new List<Point2f>();
        //            var pointsDst = new List<Point2f>();
        //            var goodMatches = new List<DMatch>();
        //            foreach (DMatch[] items in matches.Where(x => x.Length > 1))
        //            {
        //                if (items[0].Distance < 0.5 * items[1].Distance)
        //                {
        //                    pointsSrc.Add(keyPointsSrc[items[0].QueryIdx].Pt);
        //                    pointsDst.Add(keyPointsTo[items[0].TrainIdx].Pt);
        //                    goodMatches.Add(items[0]);
        //                    Console.WriteLine($"{keyPointsSrc[items[0].QueryIdx].Pt.X}, {keyPointsSrc[items[0].QueryIdx].Pt.Y}");
        //                }
        //            }
        //            var outMat = new Mat();
        //            // 算法RANSAC对匹配的结果做过滤
        //            var pSrc = pointsSrc.ConvertAll(Point2fToPoint2d);
        //            var pDst = pointsDst.ConvertAll(Point2fToPoint2d);
        //            var outMask = new Mat();
        //            // 如果原始的匹配结果为空, 则跳过过滤步骤
        //            if (pSrc.Count > 0 && pDst.Count > 0)
        //                Cv2.FindHomography(pSrc, pDst, HomographyMethods.Ransac, mask: outMask);
        //            // 如果通过RANSAC处理后的匹配点大于10个,才应用过滤. 否则使用原始的匹配点结果(匹配点过少的时候通过RANSAC处理后,可能会得到0个匹配点的结果).
        //            if (outMask.Rows > 10)
        //            {
        //                byte[] maskBytes = new byte[outMask.Rows * outMask.Cols];
        //                outMask.SetArray(0, 0, maskBytes);
        //                Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, matchesMask: maskBytes, flags: DrawMatchesFlags.NotDrawSinglePoints);
        //            }
        //            else
        //                Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, flags: DrawMatchesFlags.NotDrawSinglePoints);
        //            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(outMat);
        //        }
        //    }
        //}

        //public static Bitmap MatchPicBySurf(Bitmap imgSrc, Bitmap imgSub, double threshold = 400)
        //{
        //    using (Mat matSrc = imgSrc.ToMat())
        //    using (Mat matTo = imgSub.ToMat())
        //    using (Mat matSrcRet = new Mat())
        //    using (Mat matToRet = new Mat())
        //    {
        //        KeyPoint[] keyPointsSrc, keyPointsTo;
        //        using (var surf = OpenCvSharp.XFeatures2D.SURF.Create(threshold, 4, 3, true, true))
        //        {
        //            surf.DetectAndCompute(matSrc, null, out keyPointsSrc, matSrcRet);
        //            surf.DetectAndCompute(matTo, null, out keyPointsTo, matToRet);
        //        }
        //        using (var flnMatcher = new OpenCvSharp.FlannBasedMatcher())
        //        {
        //            var matches = flnMatcher.Match(matSrcRet, matToRet);
        //            //求最小最大距离
        //            double minDistance = 1000;//反向逼近
        //            double maxDistance = 0;
        //            for (int i = 0; i < matSrcRet.Rows; i++)
        //            {
        //                double distance = matches[i].Distance;
        //                if (distance > maxDistance)
        //                {
        //                    maxDistance = distance;
        //                }
        //                if (distance < minDistance)
        //                {
        //                    minDistance = distance;
        //                }
        //            }
        //            Console.WriteLine($"max distance : {maxDistance}");
        //            Console.WriteLine($"min distance : {minDistance}");
        //            var pointsSrc = new List<Point2f>();
        //            var pointsDst = new List<Point2f>();
        //            //筛选较好的匹配点
        //            var goodMatches = new List<DMatch>();
        //            for (int i = 0; i < matSrcRet.Rows; i++)
        //            {
        //                double distance = matches[i].Distance;
        //                if (distance < Math.Max(minDistance * 2, 0.02))
        //                {
        //                    pointsSrc.Add(keyPointsSrc[matches[i].QueryIdx].Pt);
        //                    pointsDst.Add(keyPointsTo[matches[i].TrainIdx].Pt);
        //                    //距离小于范围的压入新的DMatch
        //                    goodMatches.Add(matches[i]);
        //                }
        //            }
        //            var outMat = new Mat();
        //            // 算法RANSAC对匹配的结果做过滤
        //            var pSrc = pointsSrc.ConvertAll(Point2fToPoint2d);
        //            var pDst = pointsDst.ConvertAll(Point2fToPoint2d);

        //            var outMask = new Mat();
        //            // 如果原始的匹配结果为空, 则跳过过滤步骤
        //            if (pSrc.Count > 0 && pDst.Count > 0)
        //                Cv2.FindHomography(pSrc, pDst, HomographyMethods.Ransac, mask: outMask);
        //            // 如果通过RANSAC处理后的匹配点大于10个,才应用过滤. 否则使用原始的匹配点结果(匹配点过少的时候通过RANSAC处理后,可能会得到0个匹配点的结果).
        //            if (outMask.Rows > 10)
        //            {
        //                byte[] maskBytes = new byte[outMask.Rows * outMask.Cols];
        //                outMask.GetArray(0, 0, maskBytes);
        //                Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, matchesMask: maskBytes, flags: DrawMatchesFlags.NotDrawSinglePoints);
        //            }
        //            else
        //                Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, flags: DrawMatchesFlags.NotDrawSinglePoints);
        //            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(outMat);
        //        }
        //    }
        //}

        public static System.Drawing.Point FindPicFromImage(Bitmap imgSrc, Bitmap imgSub, double threshold = 0.9)
        {
            OpenCvSharp.Mat srcMat = null;
            OpenCvSharp.Mat dstMat = null;
            OpenCvSharp.OutputArray outArray = null;
            try
            {
                srcMat = imgSrc.ToMat();
                dstMat = imgSub.ToMat();
                outArray = OpenCvSharp.OutputArray.Create(srcMat);
                //OpenCvSharp.Cv2.MatchTemplate(srcMat, dstMat, outArray, Common.templateMatchModes);

                OpenCvSharp.Cv2.MatchTemplate(srcMat, dstMat, outArray,  TemplateMatchModes.CCoeffNormed);
                double minValue, maxValue;
                OpenCvSharp.Point location, point;
                OpenCvSharp.Cv2.MinMaxLoc(OpenCvSharp.InputArray.Create(outArray.GetMat()), out minValue, out maxValue, out location, out point);
                Console.WriteLine(maxValue);
                if (maxValue >= threshold)
                    return new System.Drawing.Point(point.X, point.Y);
                return System.Drawing.Point.Empty;
            }
            catch (Exception ex)
            {
                return System.Drawing.Point.Empty;
            }
            finally
            {
                if (srcMat != null)
                    srcMat.Dispose();
                if (dstMat != null)
                    dstMat.Dispose();
                if (outArray != null)
                    outArray.Dispose();
            }
        }
    }
}
