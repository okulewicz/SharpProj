﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Geometries;
using SparpProj.NetTopologySuite;

namespace SharpProj.Tests
{
    [TestClass]
    public class SridTests
    {
        enum Epsg //
        {
            Netherlands = 28992,
            BelgiumLambert = 3812,

            AnotherNL
        }

        [TestInitialize]
        public void Init()
        {
            SridRegister.Ensure(Epsg.Netherlands, () => CoordinateReferenceSystem.Create("EPSG:28992"), (int)Epsg.Netherlands);
            SridRegister.Ensure(Epsg.BelgiumLambert, () => CoordinateReferenceSystem.Create("EPSG:3812"), (int)Epsg.BelgiumLambert);
            SridRegister.Ensure(Epsg.AnotherNL, () => CoordinateReferenceSystem.Create("EPSG:28992"), (int)Epsg.AnotherNL); // Different EPSG, same definition. Ok. But can't use same CRS instance
        }

        [TestMethod]
        public void EpsgEnumTests()
        {
            var r = SridRegister.GetById(Epsg.Netherlands);
            var factory = r.Factory;

            //SharpProj.
            Assert.AreEqual((int)Epsg.Netherlands, r.SRID);
            Assert.AreEqual((int)Epsg.Netherlands, factory.SRID);

            Assert.AreEqual((int)Epsg.BelgiumLambert, SridRegister.GetById(Epsg.BelgiumLambert).SRID);

            Assert.AreEqual(2, r.CRS.CoordinateSystem.Axis.Count);
            Point p = factory.CreatePoint(new Coordinate(155000, 463000));

            Point pp = p.Reproject(SridRegister.GetById(Epsg.BelgiumLambert));

            Assert.AreEqual((int)Epsg.BelgiumLambert, pp.SRID);
            Assert.AreEqual(new Point(719706, 816781), new Point(pp.Coordinate.Round(0)));

            using (CoordinateTransform t = CoordinateTransform.Create(SridRegister.GetByValue(p.SRID), SridRegister.GetById(Epsg.BelgiumLambert), new CoordinateTransformOptions { NoBallparkConversions = true }))
            {
                if (t is CoordinateTransformList mc)
                {
                    Assert.AreEqual(3, mc.Count);
                    Assert.AreEqual(5, mc[0].Parameters.Count);
                    Assert.AreEqual(7, mc[1].Parameters.Count);
                    Assert.AreEqual(6, mc[2].Parameters.Count);
                }
                else
                    Assert.Fail();

                var rr = t.Apply(new PPoint(155000, 463000));

            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void NotSameTwice()
        {
            CoordinateReferenceSystem crs = SridRegister.GetById(Epsg.Netherlands);

            SridRegister.Ensure((Epsg)(-1), () => crs, -1);
        }
    }
}
