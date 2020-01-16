/*
 * Original author: Nicholas Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2018 University of Washington - Seattle, WA
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Common.Collections;
using pwiz.Skyline.Model.DocSettings.AbsoluteQuantification;
using pwiz.Skyline.Util;
using pwiz.SkylineTestUtil;

namespace pwiz.SkylineTest
{
    [TestClass]
    public class BilinearCurveFitTest : AbstractUnitTest
    {
        /// <summary>
        /// Verifies that the bilinear fit gets the same results as the original Python code.
        /// </summary>
        [TestMethod]
        public void TestBilinearFit()
        {
            CalibrationCurve calcurve = RegressionFit.BILINEAR.Fit(LKPALAVILLER_POINTS);
            Assert.IsNotNull(calcurve.TurningPoint);
            Assert.AreEqual(11673.593881022069, calcurve.TurningPoint.Value, 1);
            Assert.AreEqual(1.2771070764E-12, calcurve.Slope.Value, 1E-15);
            Assert.AreEqual(-1.4118993633E-08, calcurve.Intercept.Value, 1E-12);
        }

        [TestMethod]
        public void TestBilinearLoq()
        {
            CalibrationCurve calcurve = RegressionFit.BILINEAR.Fit(LKPALAVILLER_POINTS);
            var loq = CalibrationCurveFitter.GetBilinearLoq(calcurve, LKPALAVILLER_POINTS);
            Assert.AreEqual(29380.814749852558, loq.Value, 1);
//            Assert.IsNotNull(calcurve.TurningPoint);
//            var subset_noise = LKPALAVILLER_POINTS.Where(p => p.X < calcurve.TurningPoint).ToArray();
//            Assert.AreNotEqual(0, subset_noise.Length);
//            var dfNoise = subset_noise.Length - 2;
//            Assert.AreEqual(31, dfNoise);
//            var tStatNoise = Statistics.QT(.95, dfNoise);
//            Assert.AreEqual(1.6955, tStatNoise, .0001);
//            var yNoise = calcurve.GetY(calcurve.TurningPoint);
//            Assert.IsNotNull(yNoise);
//            var residNoise = subset_noise.Select(pt => pt.Y - yNoise.Value).ToArray();
//            var stdErrNoise = Math.Sqrt(residNoise.Sum(r => r * r) / dfNoise);
//            Assert.AreEqual(1.12629e-9, stdErrNoise, 1e-12);
//            var statsXNoise = new Statistics(subset_noise.Select(pt => pt.X));
//            var meanXNoise = statsXNoise.Mean();
//            var maxDevXNoise = calcurve.TurningPoint.Value - meanXNoise;
//            var extraFactorNoise = Math.Pow(maxDevXNoise, 2) / subset_noise.Sum(pt=>Math.Pow(pt.X - meanXNoise, 2));
//            Assert.AreEqual(0.38383093464270984, extraFactorNoise, .000001);
//            var predictIntervalNoise = tStatNoise * stdErrNoise * Math.Sqrt(1 + 1 / subset_noise.Length + extraFactorNoise);
//            Assert.AreEqual(2.246440945355845e-09, predictIntervalNoise, 1E-14);
//
//            var subset_linear = LKPALAVILLER_POINTS.Where(p => p.X >= calcurve.TurningPoint).ToArray();
//            var dfLinear = subset_linear.Length - 2;
//            Assert.AreEqual(7, dfLinear);
//            var tStatLinear = Statistics.QT(.95, dfLinear);
//            Assert.AreEqual(1.894578605061305, tStatLinear, .0001);
//            var residLinear = subset_linear.Select(pt => pt.Y - calcurve.GetY(pt.X).Value).ToArray();
//            var stdErrLinear = Math.Sqrt(residLinear.Sum(r => r * r) / dfLinear);
//            Assert.AreEqual(9.147036314447513e-09, stdErrLinear, 1E-13);
//            var meanXLinear = subset_linear.Average(pt => pt.X);
//            var maxDevXLinear = meanXLinear - calcurve.TurningPoint.Value;
//            var extraFactorLinear = Math.Pow(maxDevXLinear, 2) / subset_linear.Sum(pt => Math.Pow(pt.X - meanXLinear, 2));
//            var predictIntervalLinear =
//                tStatLinear * stdErrLinear * Math.Sqrt(1 + 1 / subset_linear.Length + extraFactorLinear);
//            Assert.AreEqual(2.0367576129195363e-08, predictIntervalLinear, 1E-12);
//            var interceptLinear = calcurve.Intercept.Value;
//            var interceptNoise = calcurve.GetY(calcurve.TurningPoint).Value;
//            var intersectPiLinear = (interceptLinear - interceptNoise - predictIntervalLinear - predictIntervalNoise) /
//                                    -calcurve.Slope.Value;
//            Assert.AreEqual(29380.814749852558, intersectPiLinear, 1);
        }

        public static readonly ImmutableList<WeightedPoint> LKPALAVILLER_POINTS = ImmutableList.ValueOf(new[]
        {
            new WeightedPoint(33100.0,4.43587139161e-08),
            new WeightedPoint(33100.0,1.95494454654e-08),
            new WeightedPoint(33100.0,2.27918101526e-08),
            new WeightedPoint(23170.0,2.30656418097e-08),
            new WeightedPoint(23170.0,5.4350527721e-09),
            new WeightedPoint(23170.0,1.23134930273e-08),
            new WeightedPoint(16550.0,1.31851525562e-08),
            new WeightedPoint(16550.0,3.38514991628e-09),
            new WeightedPoint(16550.0,7.84140959374e-09),
            new WeightedPoint(9930.0,6.02807861449e-09),
            new WeightedPoint(9930.0,4.8092607441e-10),
            new WeightedPoint(9930.0,2.59774090514e-09),
            new WeightedPoint(3310.0,7.90722974839e-10),
            new WeightedPoint(3310.0,9.17255524711e-10),
            new WeightedPoint(3310.0,4.5257191949e-10),
            new WeightedPoint(2317.0,1.36493396214e-10),
            new WeightedPoint(2317.0,6.32731302763e-10),
            new WeightedPoint(2317.0,3.39855605838e-10),
            new WeightedPoint(1655.0,1.89132927819e-10),
            new WeightedPoint(1655.0,1.69052020568e-09),
            new WeightedPoint(1655.0,1.72719025457e-10),
            new WeightedPoint(993.0,6.7422496619e-10),
            new WeightedPoint(993.0,7.82699319274e-10),
            new WeightedPoint(993.0,1.06796529348e-09),
            new WeightedPoint(331.0,9.71117065365e-10),
            new WeightedPoint(331.0,7.33700136294e-10),
            new WeightedPoint(331.0,1.01713787044e-09),
            new WeightedPoint(231.7,1.94581859484e-10),
            new WeightedPoint(231.7,8.71487280515e-10),
            new WeightedPoint(231.7,1.99592855031e-10),
            new WeightedPoint(165.5,3.1227318896e-10),
            new WeightedPoint(165.5,2.01576290459e-09),
            new WeightedPoint(165.5,0.0),
            new WeightedPoint(99.3,2.35626363707e-10),
            new WeightedPoint(99.3,9.74312769554e-10),
            new WeightedPoint(99.3,0.0),
            new WeightedPoint(33.1,0.0),
            new WeightedPoint(33.1,5.53513509328e-10),
            new WeightedPoint(33.1,1.31377272904e-10),
            new WeightedPoint(0.0,4.45448077834e-10),
            new WeightedPoint(0.0,0.0),
            new WeightedPoint(0.0,4.41809527853e-10)
        });
    }
}
