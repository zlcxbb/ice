// **********************************************************************
//
// Copyright (c) 2003-2014 ZeroC, Inc. All rights reserved.
//
// This copy of Ice is licensed to you under the terms described in the
// ICE_LICENSE file included in this distribution.
//
// **********************************************************************

using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

public class TwowaysAMI
{
    private static void test(bool b)
    {
        if(!b)
        {
            throw new System.SystemException();
        }
    }

    private class CallbackBase
    {
        internal CallbackBase()
        {
            _called = false;
        }

        public virtual void check()
        {
            lock(this)
            {
                while(!_called)
                {
                    System.Threading.Monitor.Wait(this);
                }
                _called = false;
            }
        }

        public virtual void called()
        {
            lock(this)
            {
                Debug.Assert(!_called);
                _called = true;
                System.Threading.Monitor.Pulse(this);
            }
        }

        private bool _called;
    }

    private class Callback : CallbackBase
    {
        public Callback()
        {
        }

        public Callback(Ice.Communicator c)
        {
            _communicator = c;
        }

        public Callback(int l)
        {
            _l = l;
        }

        public Callback(Dictionary<string, string> d)
        {
            _d = d;
        }

        public void ice_ping()
        {
            called();
        }

        public void ice_isA(bool r)
        {
            test(r);
            called();
        }

        public void ice_ids(string[] ids)
        {
            test(ids.Length == 3);
            called();
        }

        public void ice_id(string id)
        {
            test(id.Equals(Test.MyDerivedClass.ice_staticId()));
            called();
        }

        public void opVoid()
        {
            called();
        }

        public void opContext()
        {
            called();
        }

        public void opByte(byte r, byte b)
        {
            test(b == 0xf0);
            test(r == 0xff);
            called();
        }

        public void opBool(bool r, bool b)
        {
            test(b);
            test(!r);
            called();
        }

        public void opShortIntLong(long r, short s, int i, long l)
        {
            test(s == 10);
            test(i == 11);
            test(l == 12);
            test(r == 12);
            called();
        }

        public void opFloatDouble(double r, float f, double d)
        {
            test(f == 3.14f);
            test(d == 1.1e10);
            test(r == 1.1e10);
            called();
        }

        public void opString(string r, string s)
        {
            test(s.Equals("world hello"));
            test(r.Equals("hello world"));
            called();
        }

        public void opMyEnum(Test.MyEnum r, Test.MyEnum e)
        {
            test(e == Test.MyEnum.enum2);
            test(r == Test.MyEnum.enum3);
            called();
        }

        public void opMyClass(Test.MyClassPrx r, Test.MyClassPrx c1, Test.MyClassPrx c2)
        {
            test(c1.ice_getIdentity().Equals(_communicator.stringToIdentity("test")));
            test(c2.ice_getIdentity().Equals(_communicator.stringToIdentity("noSuchIdentity")));
            test(r.ice_getIdentity().Equals(_communicator.stringToIdentity("test")));

            //
            // We can't do the callbacks below in connection serialization mode.
            //
            if(_communicator.getProperties().getPropertyAsInt("Ice.ThreadPool.Client.Serialize") == 0)
            {
                r.opVoid();
                c1.opVoid();
                try
                {
                    c2.opVoid();
                    test(false);
                }
                catch(Ice.ObjectNotExistException)
                {
                }
            }
            called();
        }

        public void opStruct(Test.Structure rso, Test.Structure so)
        {
            test(rso.p == null);
            test(rso.e == Test.MyEnum.enum2);
            test(rso.s.s.Equals("def"));
            test(so.e == Test.MyEnum.enum3);
            test(so.s.s.Equals("a new string"));

            //
            // We can't do the callbacks below in connection serialization mode.
            //
            if(_communicator.getProperties().getPropertyAsInt("Ice.ThreadPool.Client.Serialize") == 0)
            {
                so.p.opVoid();
            }
            called();
        }

        public void opByteS(byte[] rso, byte[] bso)
        {
            test(bso.Length == 4);
            test(bso[0] == 0x22);
            test(bso[1] == 0x12);
            test(bso[2] == 0x11);
            test(bso[3] == 0x01);
            test(rso.Length == 8);
            test(rso[0] == 0x01);
            test(rso[1] == 0x11);
            test(rso[2] == 0x12);
            test(rso[3] == 0x22);
            test(rso[4] == 0xf1);
            test(rso[5] == 0xf2);
            test(rso[6] == 0xf3);
            test(rso[7] == 0xf4);
            called();
        }

        public void opBoolS(bool[] rso, bool[] bso)
        {
            test(bso.Length == 4);
            test(bso[0]);
            test(bso[1]);
            test(!bso[2]);
            test(!bso[3]);
            test(rso.Length == 3);
            test(!rso[0]);
            test(rso[1]);
            test(rso[2]);
            called();
        }

        public void opShortIntLongS(long[] rso, short[] sso, int[] iso, long[] lso)
        {
            test(sso.Length == 3);
            test(sso[0] == 1);
            test(sso[1] == 2);
            test(sso[2] == 3);
            test(iso.Length == 4);
            test(iso[0] == 8);
            test(iso[1] == 7);
            test(iso[2] == 6);
            test(iso[3] == 5);
            test(lso.Length == 6);
            test(lso[0] == 10);
            test(lso[1] == 30);
            test(lso[2] == 20);
            test(lso[3] == 10);
            test(lso[4] == 30);
            test(lso[5] == 20);
            test(rso.Length == 3);
            test(rso[0] == 10);
            test(rso[1] == 30);
            test(rso[2] == 20);
            called();
        }

        public void opFloatDoubleS(double[] rso, float[] fso, double[] dso)
        {
            test(fso.Length == 2);
            test(fso[0] == 3.14f);
            test(fso[1] == 1.11f);
            test(dso.Length == 3);
            test(dso[0] == 1.3e10);
            test(dso[1] == 1.2e10);
            test(dso[2] == 1.1e10);
            test(rso.Length == 5);
            test(rso[0] == 1.1e10);
            test(rso[1] == 1.2e10);
            test(rso[2] == 1.3e10);
            test((float) rso[3] == 3.14f);
            test((float) rso[4] == 1.11f);
            called();
        }

        public void opStringS(string[] rso, string[] sso)
        {
            test(sso.Length == 4);
            test(sso[0].Equals("abc"));
            test(sso[1].Equals("de"));
            test(sso[2].Equals("fghi"));
            test(sso[3].Equals("xyz"));
            test(rso.Length == 3);
            test(rso[0].Equals("fghi"));
            test(rso[1].Equals("de"));
            test(rso[2].Equals("abc"));
            called();
        }

        public void opByteSS(byte[][] rso, byte[][] bso)
        {
            test(bso.Length == 2);
            test(bso[0].Length == 1);
            test(bso[0][0] == 0xff);
            test(bso[1].Length == 3);
            test(bso[1][0] == 0x01);
            test(bso[1][1] == 0x11);
            test(bso[1][2] == 0x12);
            test(rso.Length == 4);
            test(rso[0].Length == 3);
            test(rso[0][0] == 0x01);
            test(rso[0][1] == 0x11);
            test(rso[0][2] == 0x12);
            test(rso[1].Length == 1);
            test(rso[1][0] == 0xff);
            test(rso[2].Length == 1);
            test(rso[2][0] == 0x0e);
            test(rso[3].Length == 2);
            test(rso[3][0] == 0xf2);
            test(rso[3][1] == 0xf1);
            called();
        }

        public void opBoolSS(bool[][] rso, bool[][] bso)
        {
            test(bso.Length == 4);
            test(bso[0].Length == 1);
            test(bso[0][0]);
            test(bso[1].Length == 1);
            test(!bso[1][0]);
            test(bso[2].Length == 2);
            test(bso[2][0]);
            test(bso[2][1]);
            test(bso[3].Length == 3);
            test(!bso[3][0]);
            test(!bso[3][1]);
            test(bso[3][2]);
            test(rso.Length == 3);
            test(rso[0].Length == 2);
            test(rso[0][0]);
            test(rso[0][1]);
            test(rso[1].Length == 1);
            test(!rso[1][0]);
            test(rso[2].Length == 1);
            test(rso[2][0]);
            called();
        }

        public void opShortIntLongSS(long[][] rso, short[][] sso, int[][] iso, long[][] lso)
        {
            test(rso.Length == 1);
            test(rso[0].Length == 2);
            test(rso[0][0] == 496);
            test(rso[0][1] == 1729);
            test(sso.Length == 3);
            test(sso[0].Length == 3);
            test(sso[0][0] == 1);
            test(sso[0][1] == 2);
            test(sso[0][2] == 5);
            test(sso[1].Length == 1);
            test(sso[1][0] == 13);
            test(sso[2].Length == 0);
            test(iso.Length == 2);
            test(iso[0].Length == 1);
            test(iso[0][0] == 42);
            test(iso[1].Length == 2);
            test(iso[1][0] == 24);
            test(iso[1][1] == 98);
            test(lso.Length == 2);
            test(lso[0].Length == 2);
            test(lso[0][0] == 496);
            test(lso[0][1] == 1729);
            test(lso[1].Length == 2);
            test(lso[1][0] == 496);
            test(lso[1][1] == 1729);
            called();
        }

        public void opFloatDoubleSS(double[][] rso, float[][] fso, double[][] dso)
        {
            test(fso.Length == 3);
            test(fso[0].Length == 1);
            test(fso[0][0] == 3.14f);
            test(fso[1].Length == 1);
            test(fso[1][0] == 1.11f);
            test(fso[2].Length == 0);
            test(dso.Length == 1);
            test(dso[0].Length == 3);
            test(dso[0][0] == 1.1e10);
            test(dso[0][1] == 1.2e10);
            test(dso[0][2] == 1.3e10);
            test(rso.Length == 2);
            test(rso[0].Length == 3);
            test(rso[0][0] == 1.1e10);
            test(rso[0][1] == 1.2e10);
            test(rso[0][2] == 1.3e10);
            test(rso[1].Length == 3);
            test(rso[1][0] == 1.1e10);
            test(rso[1][1] == 1.2e10);
            test(rso[1][2] == 1.3e10);
            called();
        }

        public void opStringSS(string[][] rso, string[][] sso)
        {
            test(sso.Length == 5);
            test(sso[0].Length == 1);
            test(sso[0][0].Equals("abc"));
            test(sso[1].Length == 2);
            test(sso[1][0].Equals("de"));
            test(sso[1][1].Equals("fghi"));
            test(sso[2].Length == 0);
            test(sso[3].Length == 0);
            test(sso[4].Length == 1);
            test(sso[4][0].Equals("xyz"));
            test(rso.Length == 3);
            test(rso[0].Length == 1);
            test(rso[0][0].Equals("xyz"));
            test(rso[1].Length == 0);
            test(rso[2].Length == 0);
            called();
        }

        public void opStringSSS(string[][][] rsso, string[][][] ssso)
        {
            test(ssso.Length == 5);
            test(ssso[0].Length == 2);
            test(ssso[0][0].Length == 2);
            test(ssso[0][1].Length == 1);
            test(ssso[1].Length == 1);
            test(ssso[1][0].Length == 1);
            test(ssso[2].Length == 2);
            test(ssso[2][0].Length == 2);
            test(ssso[2][1].Length == 1);
            test(ssso[3].Length == 1);
            test(ssso[3][0].Length == 1);
            test(ssso[4].Length == 0);
            test(ssso[0][0][0].Equals("abc"));
            test(ssso[0][0][1].Equals("de"));
            test(ssso[0][1][0].Equals("xyz"));
            test(ssso[1][0][0].Equals("hello"));
            test(ssso[2][0][0].Equals(""));
            test(ssso[2][0][1].Equals(""));
            test(ssso[2][1][0].Equals("abcd"));
            test(ssso[3][0][0].Equals(""));

            test(rsso.Length == 3);
            test(rsso[0].Length == 0);
            test(rsso[1].Length == 1);
            test(rsso[1][0].Length == 1);
            test(rsso[2].Length == 2);
            test(rsso[2][0].Length == 2);
            test(rsso[2][1].Length == 1);
            test(rsso[1][0][0].Equals(""));
            test(rsso[2][0][0].Equals(""));
            test(rsso[2][0][1].Equals(""));
            test(rsso[2][1][0].Equals("abcd"));
            called();
        }

        public void opByteBoolD(Dictionary<byte, bool> ro, Dictionary<byte, bool> _do)
        {
            Dictionary<byte, bool> di1 = new Dictionary<byte, bool>();
            di1[10] = true;
            di1[100] = false;
            test(Ice.CollectionComparer.Equals(_do, di1));
            test(ro.Count == 4);
            // test(ro[10] == true); // Disabled since new dictionary mapping.
            test(ro[11] == false);
            test(ro[100] == false);
            test(ro[101] == true);
            called();
        }

        public void opShortIntD(Dictionary<short, int> ro, Dictionary<short, int> _do)
        {
            Dictionary<short, int> di1 = new Dictionary<short, int>();
            di1[110] = -1;
            di1[1100] = 123123;
            test(Ice.CollectionComparer.Equals(_do, di1));
            test(ro.Count == 4);
            // test(ro[110] == -1); // Disabled since new dictionary mapping.
            test(ro[111] == -100);
            test(ro[1100] == 123123);
            test(ro[1101] == 0);
            called();
        }

        public void opLongFloatD(Dictionary<long, float> ro, Dictionary<long, float> _do)
        {
            Dictionary<long, float> di1 = new Dictionary<long, float>();
            di1[999999110L] = -1.1f;
            di1[999999111L] = 123123.2f;
            test(Ice.CollectionComparer.Equals(_do, di1));
            test(ro.Count == 4);
            test(ro[999999110L] == -1.1f);
            test(ro[999999120L] == -100.4f);
            test(ro[999999111L] == 123123.2f);
            test(ro[999999130L] == 0.5f);
            called();
        }

        public void opStringStringD(Dictionary<string, string> ro, Dictionary<string, string> _do)
        {
            Dictionary<string, string> di1 = new Dictionary<string, string>();
            di1["foo"] = "abc -1.1";
            di1["bar"] = "abc 123123.2";
            test(Ice.CollectionComparer.Equals(_do, di1));
            test(ro.Count == 4);
            test(ro["foo"].Equals("abc -1.1"));
            test(ro["FOO"].Equals("abc -100.4"));
            test(ro["bar"].Equals("abc 123123.2"));
            test(ro["BAR"].Equals("abc 0.5"));
            called();
        }

        public void opStringMyEnumD(Dictionary<string, Test.MyEnum> ro, Dictionary<string, Test.MyEnum> _do)
        {
            Dictionary<string, Test.MyEnum> di1 = new Dictionary<string, Test.MyEnum>();
            di1["abc"] = Test.MyEnum.enum1;
            di1[""] = Test.MyEnum.enum2;
            test(Ice.CollectionComparer.Equals(_do, di1));
            test(ro.Count == 4);
            test(ro["abc"] == Test.MyEnum.enum1);
            test(ro["qwerty"] == Test.MyEnum.enum3);
            test(ro[""] == Test.MyEnum.enum2);
            test(ro["Hello!!"] == Test.MyEnum.enum2);
            called();
        }

        public void opMyEnumStringD(Dictionary<Test.MyEnum, string> ro, Dictionary<Test.MyEnum, string> _do)
        {
            Dictionary<Test.MyEnum, string> di1 = new Dictionary<Test.MyEnum, string>();
            di1[Test.MyEnum.enum1] = "abc";
            test(Ice.CollectionComparer.Equals(_do, di1));
            test(ro.Count == 3);
            test(ro[Test.MyEnum.enum1].Equals("abc"));
            test(ro[Test.MyEnum.enum2].Equals("Hello!!"));
            test(ro[Test.MyEnum.enum3].Equals("qwerty"));
            called();
        }

        public void opMyStructMyEnumD(Dictionary<Test.MyStruct, Test.MyEnum> ro,
                                      Dictionary<Test.MyStruct, Test.MyEnum> _do)
        {
            Test.MyStruct s11 = new Test.MyStruct(1, 1);
            Test.MyStruct s12 = new Test.MyStruct(1, 2);
            Dictionary<Test.MyStruct, Test.MyEnum> di1 = new Dictionary<Test.MyStruct, Test.MyEnum>();
            di1[s11] = Test.MyEnum.enum1;
            di1[s12] = Test.MyEnum.enum2;
            test(Ice.CollectionComparer.Equals(_do, di1));
            Test.MyStruct s22 = new Test.MyStruct(2, 2);
            Test.MyStruct s23 = new Test.MyStruct(2, 3);
            test(ro.Count == 4);
            test(ro[s11] == Test.MyEnum.enum1);
            test(ro[s12] == Test.MyEnum.enum2);
            test(ro[s22] == Test.MyEnum.enum3);
            test(ro[s23] == Test.MyEnum.enum2);
            called();
        }

        public void opIntS(int[] r)
        {
            test(r.Length == _l);
            for(int j = 0; j < r.Length; ++j)
            {
                test(r[j] == -j);
            }
            called();
        }

        public void opContextNotEqual(Dictionary<string, string> r)
        {
            test(!Ice.CollectionComparer.Equals(r, _d));
            called();
        }

        public void opContextEqual(Dictionary<string, string> r)
        {
            test(Ice.CollectionComparer.Equals(r, _d));
            called();
        }

        public void opIdempotent()
        {
            called();
        }

        public void opNonmutating()
        {
            called();
        }

        public void opDerived()
        {
            called();
        }

        public void exCB(Ice.Exception ex)
        {
            test(false);
        }

        private Ice.Communicator _communicator;
        private int _l;
        private Dictionary<string, string> _d;
    }

    internal static void twowaysAMI(Ice.Communicator communicator, Test.MyClassPrx p)
    {
        {
            Ice.AsyncResult r = p.begin_ice_ping();
            p.end_ice_ping(r);
        }

        {
            Callback cb = new Callback();
            p.begin_ice_ping().whenCompleted(cb.ice_ping, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_ice_ping().whenCompleted(
                () =>
                {
                    cb.ice_ping();
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Ice.AsyncResult r = p.begin_ice_isA(Test.MyClass.ice_staticId());
            test(p.end_ice_isA(r));
        }

        {
            Callback cb = new Callback();
            p.begin_ice_isA(Test.MyClass.ice_staticId()).whenCompleted(cb.ice_isA, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_ice_isA(Test.MyClass.ice_staticId()).whenCompleted(
                (bool v) =>
                {
                    cb.ice_isA(v);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Ice.AsyncResult r = p.begin_ice_ids();
            test(p.end_ice_ids(r).Length == 3);
        }

        {
            Callback cb = new Callback();
            p.begin_ice_ids().whenCompleted(cb.ice_ids, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_ice_ids().whenCompleted(
                (string[] ids) =>
                {
                    cb.ice_ids(ids);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Ice.AsyncResult r = p.begin_ice_id();
            test(p.end_ice_id(r).Equals(Test.MyDerivedClass.ice_staticId()));
        }

        {
            Callback cb = new Callback();
            p.begin_ice_id().whenCompleted(cb.ice_id, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_ice_id().whenCompleted(
                (string id) =>
                {
                    cb.ice_id(id);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Ice.AsyncResult r = p.begin_opVoid();
            p.end_opVoid(r);
        }

        {
            Callback cb = new Callback();
            p.begin_opVoid().whenCompleted(cb.opVoid, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opVoid().whenCompleted(
                () =>
                {
                    cb.opVoid();
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Ice.AsyncResult r = p.begin_opByte(0xff, 0x0f);
            byte p3;
            byte ret = p.end_opByte(out p3, r);
            test(p3 == 0xf0);
            test(ret == 0xff);
        }

        {
            Callback cb = new Callback();
            p.begin_opByte(0xff, 0x0f).whenCompleted(cb.opByte, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opByte(0xff, 0x0f).whenCompleted(
                (byte r, byte b) =>
                {
                    cb.opByte(r, b);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opBool(true, false).whenCompleted(cb.opBool, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opBool(true, false).whenCompleted(
                (bool r, bool b) =>
                {
                    cb.opBool(r, b);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opShortIntLong(10, 11, 12).whenCompleted(cb.opShortIntLong, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opShortIntLong(10, 11, 12).whenCompleted(
                (long r, short s, int i, long l) =>
                {
                    cb.opShortIntLong(r, s, i, l);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opFloatDouble(3.14f, 1.1E10).whenCompleted(cb.opFloatDouble, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opFloatDouble(3.14f, 1.1E10).whenCompleted(
                (double r, float f, double d) =>
                {
                    cb.opFloatDouble(r, f, d);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opString("hello", "world").whenCompleted(cb.opString, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opString("hello", "world").whenCompleted(
                (string r, string s) =>
                {
                    cb.opString(r, s);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opMyEnum(Test.MyEnum.enum2).whenCompleted(cb.opMyEnum, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opMyEnum(Test.MyEnum.enum2).whenCompleted(
                (Test.MyEnum r, Test.MyEnum e) =>
                {
                    cb.opMyEnum(r, e);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Callback cb = new Callback(communicator);
            p.begin_opMyClass(p).whenCompleted(cb.opMyClass, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback(communicator);
            p.begin_opMyClass(p).whenCompleted(
                (Test.MyClassPrx r, Test.MyClassPrx c1, Test.MyClassPrx c2) =>
                {
                    cb.opMyClass(r, c1, c2);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Test.Structure si1 = new Test.Structure();
            si1.p = p;
            si1.e = Test.MyEnum.enum3;
            si1.s = new Test.AnotherStruct();
            si1.s.s = "abc";
            Test.Structure si2 = new Test.Structure();
            si2.p = null;
            si2.e = Test.MyEnum.enum2;
            si2.s = new Test.AnotherStruct();
            si2.s.s = "def";

            Callback cb = new Callback(communicator);
            p.begin_opStruct(si1, si2).whenCompleted(cb.opStruct, cb.exCB);
            cb.check();
        }

        {
            Test.Structure si1 = new Test.Structure();
            si1.p = p;
            si1.e = Test.MyEnum.enum3;
            si1.s = new Test.AnotherStruct();
            si1.s.s = "abc";
            Test.Structure si2 = new Test.Structure();
            si2.p = null;
            si2.e = Test.MyEnum.enum2;
            si2.s = new Test.AnotherStruct();
            si2.s.s = "def";

            Callback cb = new Callback(communicator);
            p.begin_opStruct(si1, si2).whenCompleted(
                (Test.Structure rso, Test.Structure so) =>
                {
                    cb.opStruct(rso, so);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            byte[] bsi1 = new byte[] { 0x01, 0x11, 0x12, 0x22 };
            byte[] bsi2 = new byte[] { 0xf1, 0xf2, 0xf3, 0xf4 };

            Callback cb = new Callback();
            p.begin_opByteS(bsi1, bsi2).whenCompleted(cb.opByteS, cb.exCB);
            cb.check();
        }

        {
            byte[] bsi1 = new byte[] { 0x01, 0x11, 0x12, 0x22 };
            byte[] bsi2 = new byte[] { 0xf1, 0xf2, 0xf3, 0xf4 };

            Callback cb = new Callback();
            p.begin_opByteS(bsi1, bsi2).whenCompleted(
                (byte[] rso, byte[] bso) =>
                {
                    cb.opByteS(rso, bso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            bool[] bsi1 = new bool[] { true, true, false };
            bool[] bsi2 = new bool[] { false };

            Callback cb = new Callback();
            p.begin_opBoolS(bsi1, bsi2).whenCompleted(cb.opBoolS, cb.exCB);
            cb.check();
        }

        {
            bool[] bsi1 = new bool[] { true, true, false };
            bool[] bsi2 = new bool[] { false };

            Callback cb = new Callback();
            p.begin_opBoolS(bsi1, bsi2).whenCompleted(
                (bool[] rso, bool[] bso) =>
                {
                    cb.opBoolS(rso, bso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            short[] ssi = new short[] { 1, 2, 3 };
            int[] isi = new int[] { 5, 6, 7, 8 };
            long[] lsi = new long[] { 10, 30, 20 };

            Callback cb = new Callback();
            p.begin_opShortIntLongS(ssi, isi, lsi).whenCompleted(cb.opShortIntLongS, cb.exCB);
            cb.check();
        }

        {
            short[] ssi = new short[] { 1, 2, 3 };
            int[] isi = new int[] { 5, 6, 7, 8 };
            long[] lsi = new long[] { 10, 30, 20 };

            Callback cb = new Callback();
            p.begin_opShortIntLongS(ssi, isi, lsi).whenCompleted(
                (long[] rso, short[] sso, int[] iso, long[] lso) =>
                {
                    cb.opShortIntLongS(rso, sso, iso, lso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            float[] fsi = new float[] { 3.14f, 1.11f };
            double[] dsi = new double[] { 1.1e10, 1.2e10, 1.3e10 };

            Callback cb = new Callback();
            p.begin_opFloatDoubleS(fsi, dsi).whenCompleted(cb.opFloatDoubleS, cb.exCB);
            cb.check();
        }

        {
            float[] fsi = new float[] { 3.14f, 1.11f };
            double[] dsi = new double[] { 1.1e10, 1.2e10, 1.3e10 };

            Callback cb = new Callback();
            p.begin_opFloatDoubleS(fsi, dsi).whenCompleted(
                (double[] rso, float[] fso, double[] dso) =>
                {
                    cb.opFloatDoubleS(rso, fso, dso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            string[] ssi1 = new string[] { "abc", "de", "fghi" };
            string[] ssi2 = new string[] { "xyz" };

            Callback cb = new Callback();
            p.begin_opStringS(ssi1, ssi2).whenCompleted(cb.opStringS, cb.exCB);
            cb.check();
        }

        {
            string[] ssi1 = new string[] { "abc", "de", "fghi" };
            string[] ssi2 = new string[] { "xyz" };

            Callback cb = new Callback();
            p.begin_opStringS(ssi1, ssi2).whenCompleted(
                (string[] rso, string[] sso) =>
                {
                    cb.opStringS(rso, sso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            byte[] s11 = new byte[] { 0x01, 0x11, 0x12 };
            byte[] s12 = new byte[] { 0xff };
            byte[][] bsi1 = new byte[][] { s11, s12 };

            byte[] s21 = new byte[] { 0x0e };
            byte[] s22 = new byte[] { 0xf2, 0xf1 };
            byte[][] bsi2 = new byte[][] { s21, s22 };

            Callback cb = new Callback();
            p.begin_opByteSS(bsi1, bsi2).whenCompleted(cb.opByteSS, cb.exCB);
            cb.check();
        }

        {
            byte[] s11 = new byte[] { 0x01, 0x11, 0x12 };
            byte[] s12 = new byte[] { 0xff };
            byte[][] bsi1 = new byte[][] { s11, s12 };

            byte[] s21 = new byte[] { 0x0e };
            byte[] s22 = new byte[] { 0xf2, 0xf1 };
            byte[][] bsi2 = new byte[][] { s21, s22 };

            Callback cb = new Callback();
            p.begin_opByteSS(bsi1, bsi2).whenCompleted(
                (byte[][] rso, byte[][] bso) =>
                {
                    cb.opByteSS(rso, bso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            bool[] s11 = new bool[] {true};
            bool[] s12 = new bool[] {false};
            bool[] s13 = new bool[] {true, true};
            bool[][] bsi1 = new bool[][] {s11, s12, s13};

            bool[] s21 = new bool[] {false, false, true};
            bool[][] bsi2 = new bool[][] {s21};

            Callback cb = new Callback();
            p.begin_opBoolSS(bsi1, bsi2).whenCompleted(cb.opBoolSS, cb.exCB);
            cb.check();
        }

        {
            bool[] s11 = new bool[] {true};
            bool[] s12 = new bool[] {false};
            bool[] s13 = new bool[] {true, true};
            bool[][] bsi1 = new bool[][] {s11, s12, s13};

            bool[] s21 = new bool[] {false, false, true};
            bool[][] bsi2 = new bool[][] {s21};

            Callback cb = new Callback();
            p.begin_opBoolSS(bsi1, bsi2).whenCompleted(
                (bool[][] rso, bool[][] bso) =>
                {
                    cb.opBoolSS(rso, bso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            short[] s11 = new short[] {1, 2, 5};
            short[] s12 = new short[] {13};
            short[] s13 = new short[] {};
            short[][] ssi = new short[][] {s11, s12, s13};

            int[] i11 = new int[] {24, 98};
            int[] i12 = new int[] {42};
            int[][] isi = new int[][] {i11, i12};

            long[] l11 = new long[] {496, 1729};
            long[][] lsi = new long[][] {l11};

            Callback cb = new Callback();
            p.begin_opShortIntLongSS(ssi, isi, lsi).whenCompleted(cb.opShortIntLongSS, cb.exCB);
            cb.check();
        }

        {
            short[] s11 = new short[] {1, 2, 5};
            short[] s12 = new short[] {13};
            short[] s13 = new short[] {};
            short[][] ssi = new short[][] {s11, s12, s13};

            int[] i11 = new int[] {24, 98};
            int[] i12 = new int[] {42};
            int[][] isi = new int[][] {i11, i12};

            long[] l11 = new long[] {496, 1729};
            long[][] lsi = new long[][] {l11};

            Callback cb = new Callback();
            p.begin_opShortIntLongSS(ssi, isi, lsi).whenCompleted(
                (long[][] rso, short[][] sso, int[][] iso, long[][] lso) =>
                {
                    cb.opShortIntLongSS(rso, sso, iso, lso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            float[] f11 = new float[] { 3.14f };
            float[] f12 = new float[] { 1.11f };
            float[] f13 = new float[] { };
            float[][] fsi = new float[][] { f11, f12, f13 };

            double[] d11 = new double[] { 1.1e10, 1.2e10, 1.3e10 };
            double[][] dsi = new double[][] { d11 };

            Callback cb = new Callback();
            p.begin_opFloatDoubleSS(fsi, dsi).whenCompleted(cb.opFloatDoubleSS, cb.exCB);
            cb.check();
        }

        {
            float[] f11 = new float[] { 3.14f };
            float[] f12 = new float[] { 1.11f };
            float[] f13 = new float[] { };
            float[][] fsi = new float[][] { f11, f12, f13 };

            double[] d11 = new double[] { 1.1e10, 1.2e10, 1.3e10 };
            double[][] dsi = new double[][] { d11 };

            Callback cb = new Callback();
            p.begin_opFloatDoubleSS(fsi, dsi).whenCompleted(
                (double[][] rso, float[][] fso, double[][] dso) =>
                {
                    cb.opFloatDoubleSS(rso, fso, dso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            string[] s11 = new string[] { "abc" };
            string[] s12 = new string[] { "de", "fghi" };
            string[][] ssi1 = new string[][] { s11, s12 };

            string[] s21 = new string[] {};
            string[] s22 = new string[] {};
            string[] s23 = new string[] { "xyz" };
            string[][] ssi2 = new string[][] { s21, s22, s23 };

            Callback cb = new Callback();
            p.begin_opStringSS(ssi1, ssi2).whenCompleted(cb.opStringSS, cb.exCB);
            cb.check();
        }

        {
            string[] s11 = new string[] { "abc" };
            string[] s12 = new string[] { "de", "fghi" };
            string[][] ssi1 = new string[][] { s11, s12 };

            string[] s21 = new string[] {};
            string[] s22 = new string[] {};
            string[] s23 = new string[] { "xyz" };
            string[][] ssi2 = new string[][] { s21, s22, s23 };

            Callback cb = new Callback();
            p.begin_opStringSS(ssi1, ssi2).whenCompleted(
                (string[][] rso, string[][] sso) =>
                {
                    cb.opStringSS(rso, sso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            string[] s111 = new string[] { "abc", "de"};
            string[] s112 = new string[] { "xyz" };
            string[][] ss11 = new string[][] { s111, s112 };
            string[] s121 = new string[] { "hello"};
            string[][] ss12 = new string[][] { s121 };
            string[][][] sssi1 = new string[][][] { ss11, ss12 };

            string[] s211 = new string[] { "", ""};
            string[] s212 = new string[] { "abcd" };
            string[][] ss21 = new string[][] { s211, s212 };
            string[] s221 = new string[] { ""};
            string[][] ss22 = new string[][] { s221 };
            string[][] ss23 = new string[][] {};
            string[][][] sssi2 = new string[][][] { ss21, ss22, ss23 };

            Callback cb = new Callback();
            p.begin_opStringSSS(sssi1, sssi2).whenCompleted(cb.opStringSSS, cb.exCB);
            cb.check();
        }

        {
            string[] s111 = new string[] { "abc", "de"};
            string[] s112 = new string[] { "xyz" };
            string[][] ss11 = new string[][] { s111, s112 };
            string[] s121 = new string[] { "hello"};
            string[][] ss12 = new string[][] { s121 };
            string[][][] sssi1 = new string[][][] { ss11, ss12 };

            string[] s211 = new string[] { "", ""};
            string[] s212 = new string[] { "abcd" };
            string[][] ss21 = new string[][] { s211, s212 };
            string[] s221 = new string[] { ""};
            string[][] ss22 = new string[][] { s221 };
            string[][] ss23 = new string[][] {};
            string[][][] sssi2 = new string[][][] { ss21, ss22, ss23 };

            Callback cb = new Callback();
            p.begin_opStringSSS(sssi1, sssi2).whenCompleted(
                (string[][][] rsso, string[][][] ssso) =>
                {
                    cb.opStringSSS(rsso, ssso);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Dictionary<byte, bool> di1 = new Dictionary<byte, bool>();
            di1[10] = true;
            di1[100] = false;
            Dictionary<byte, bool> di2 = new Dictionary<byte, bool>();
            di2[10] = true;
            di2[11] = false;
            di2[101] = true;

            Callback cb = new Callback();
            p.begin_opByteBoolD(di1, di2).whenCompleted(cb.opByteBoolD, cb.exCB);
            cb.check();
        }

        {
            Dictionary<byte, bool> di1 = new Dictionary<byte, bool>();
            di1[10] = true;
            di1[100] = false;
            Dictionary<byte, bool> di2 = new Dictionary<byte, bool>();
            di2[10] = true;
            di2[11] = false;
            di2[101] = true;

            Callback cb = new Callback();
            p.begin_opByteBoolD(di1, di2).whenCompleted(
                (Dictionary<byte, bool> ro, Dictionary<byte, bool> _do) =>
                {
                    cb.opByteBoolD(ro, _do);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Dictionary<short, int> di1 = new Dictionary<short, int>();
            di1[110] = -1;
            di1[1100] = 123123;
            Dictionary<short, int> di2 = new Dictionary<short, int>();
            di2[110] = -1;
            di2[111] = -100;
            di2[1101] = 0;

            Callback cb = new Callback();
            p.begin_opShortIntD(di1, di2).whenCompleted(cb.opShortIntD, cb.exCB);
            cb.check();
        }

        {
            Dictionary<short, int> di1 = new Dictionary<short, int>();
            di1[110] = -1;
            di1[1100] = 123123;
            Dictionary<short, int> di2 = new Dictionary<short, int>();
            di2[110] = -1;
            di2[111] = -100;
            di2[1101] = 0;

            Callback cb = new Callback();
            p.begin_opShortIntD(di1, di2).whenCompleted(
                (Dictionary<short, int> ro, Dictionary<short, int> _do) =>
                {
                    cb.opShortIntD(ro, _do);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Dictionary<long, float> di1 = new Dictionary<long, float>();
            di1[999999110L] = -1.1f;
            di1[999999111L] = 123123.2f;
            Dictionary<long, float> di2 = new Dictionary<long, float>();
            di2[999999110L] = -1.1f;
            di2[999999120L] = -100.4f;
            di2[999999130L] = 0.5f;

            Callback cb = new Callback();
            p.begin_opLongFloatD(di1, di2).whenCompleted(cb.opLongFloatD, cb.exCB);
            cb.check();
        }

        {
            Dictionary<long, float> di1 = new Dictionary<long, float>();
            di1[999999110L] = -1.1f;
            di1[999999111L] = 123123.2f;
            Dictionary<long, float> di2 = new Dictionary<long, float>();
            di2[999999110L] = -1.1f;
            di2[999999120L] = -100.4f;
            di2[999999130L] = 0.5f;

            Callback cb = new Callback();
            p.begin_opLongFloatD(di1, di2).whenCompleted(
                (Dictionary<long, float> ro, Dictionary<long, float> _do) =>
                {
                    cb.opLongFloatD(ro, _do);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Dictionary<string, string> di1 = new Dictionary<string, string>();
            di1["foo"] = "abc -1.1";
            di1["bar"] = "abc 123123.2";
            Dictionary<string, string> di2 = new Dictionary<string, string>();
            di2["foo"] = "abc -1.1";
            di2["FOO"] = "abc -100.4";
            di2["BAR"] = "abc 0.5";

            Callback cb = new Callback();
            p.begin_opStringStringD(di1, di2).whenCompleted(cb.opStringStringD, cb.exCB);
            cb.check();
        }

        {
            Dictionary<string, string> di1 = new Dictionary<string, string>();
            di1["foo"] = "abc -1.1";
            di1["bar"] = "abc 123123.2";
            Dictionary<string, string> di2 = new Dictionary<string, string>();
            di2["foo"] = "abc -1.1";
            di2["FOO"] = "abc -100.4";
            di2["BAR"] = "abc 0.5";

            Callback cb = new Callback();
            p.begin_opStringStringD(di1, di2).whenCompleted(
                (Dictionary<string, string> ro, Dictionary<string, string> _do) =>
                {
                    cb.opStringStringD(ro, _do);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Dictionary<string, Test.MyEnum> di1 = new Dictionary<string, Test.MyEnum>();
            di1["abc"] = Test.MyEnum.enum1;
            di1[""] = Test.MyEnum.enum2;
            Dictionary<string, Test.MyEnum> di2 = new Dictionary<string, Test.MyEnum>();
            di2["abc"] = Test.MyEnum.enum1;
            di2["qwerty"] = Test.MyEnum.enum3;
            di2["Hello!!"] = Test.MyEnum.enum2;

            Callback cb = new Callback();
            p.begin_opStringMyEnumD(di1, di2).whenCompleted(cb.opStringMyEnumD, cb.exCB);
            cb.check();
        }

        {
            Dictionary<string, Test.MyEnum> di1 = new Dictionary<string, Test.MyEnum>();
            di1["abc"] = Test.MyEnum.enum1;
            di1[""] = Test.MyEnum.enum2;
            Dictionary<string, Test.MyEnum> di2 = new Dictionary<string, Test.MyEnum>();
            di2["abc"] = Test.MyEnum.enum1;
            di2["qwerty"] = Test.MyEnum.enum3;
            di2["Hello!!"] = Test.MyEnum.enum2;

            Callback cb = new Callback();
            p.begin_opStringMyEnumD(di1, di2).whenCompleted(
                (Dictionary<string, Test.MyEnum> ro, Dictionary<string, Test.MyEnum> _do) =>
                {
                    cb.opStringMyEnumD(ro, _do);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Dictionary<Test.MyEnum, string> di1 = new Dictionary<Test.MyEnum, string>();
            di1[Test.MyEnum.enum1] = "abc";
            Dictionary<Test.MyEnum, string> di2 = new Dictionary<Test.MyEnum, string>();
            di2[Test.MyEnum.enum2] = "Hello!!";
            di2[Test.MyEnum.enum3] = "qwerty";

            Callback cb = new Callback();
            p.begin_opMyEnumStringD(di1, di2).whenCompleted(cb.opMyEnumStringD, cb.exCB);
            cb.check();
        }

        {
            Dictionary<Test.MyEnum, string> di1 = new Dictionary<Test.MyEnum, string>();
            di1[Test.MyEnum.enum1] = "abc";
            Dictionary<Test.MyEnum, string> di2 = new Dictionary<Test.MyEnum, string>();
            di2[Test.MyEnum.enum2] = "Hello!!";
            di2[Test.MyEnum.enum3] = "qwerty";

            Callback cb = new Callback();
            p.begin_opMyEnumStringD(di1, di2).whenCompleted(
                (Dictionary<Test.MyEnum, string> ro, Dictionary<Test.MyEnum, string> _do) =>
                {
                    cb.opMyEnumStringD(ro, _do);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Test.MyStruct s11 = new Test.MyStruct(1, 1);
            Test.MyStruct s12 = new Test.MyStruct(1, 2);
            Dictionary<Test.MyStruct, Test.MyEnum> di1 = new Dictionary<Test.MyStruct, Test.MyEnum>();
            di1[s11] = Test.MyEnum.enum1;
            di1[s12] = Test.MyEnum.enum2;

            Test.MyStruct s22 = new Test.MyStruct(2, 2);
            Test.MyStruct s23 = new Test.MyStruct(2, 3);
            Dictionary<Test.MyStruct, Test.MyEnum> di2 = new Dictionary<Test.MyStruct, Test.MyEnum>();
            di2[s11] = Test.MyEnum.enum1;
            di2[s22] = Test.MyEnum.enum3;
            di2[s23] = Test.MyEnum.enum2;

            Callback cb = new Callback();
            p.begin_opMyStructMyEnumD(di1, di2).whenCompleted(cb.opMyStructMyEnumD, cb.exCB);
            cb.check();
        }

        {
            Test.MyStruct s11 = new Test.MyStruct(1, 1);
            Test.MyStruct s12 = new Test.MyStruct(1, 2);
            Dictionary<Test.MyStruct, Test.MyEnum> di1 = new Dictionary<Test.MyStruct, Test.MyEnum>();
            di1[s11] = Test.MyEnum.enum1;
            di1[s12] = Test.MyEnum.enum2;

            Test.MyStruct s22 = new Test.MyStruct(2, 2);
            Test.MyStruct s23 = new Test.MyStruct(2, 3);
            Dictionary<Test.MyStruct, Test.MyEnum> di2 = new Dictionary<Test.MyStruct, Test.MyEnum>();
            di2[s11] = Test.MyEnum.enum1;
            di2[s22] = Test.MyEnum.enum3;
            di2[s23] = Test.MyEnum.enum2;

            Callback cb = new Callback();
            p.begin_opMyStructMyEnumD(di1, di2).whenCompleted(
                (Dictionary<Test.MyStruct, Test.MyEnum> ro,
                 Dictionary<Test.MyStruct, Test.MyEnum> _do) =>
                {
                    cb.opMyStructMyEnumD(ro, _do);
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            int[] lengths = new int[] { 0, 1, 2, 126, 127, 128, 129, 253, 254, 255, 256, 257, 1000 };

            for(int l = 0; l < lengths.Length; ++l)
            {
                int[] s = new int[lengths[l]];
                for(int i = 0; i < lengths[l]; ++i)
                {
                    s[i] = i;
                }

                Callback cb = new Callback(lengths[l]);
                p.begin_opIntS(s).whenCompleted(cb.opIntS, cb.exCB);
                cb.check();
            }
        }

        {
            int[] lengths = new int[] { 0, 1, 2, 126, 127, 128, 129, 253, 254, 255, 256, 257, 1000 };

            for(int l = 0; l < lengths.Length; ++l)
            {
                int[] s = new int[lengths[l]];
                for(int i = 0; i < lengths[l]; ++i)
                {
                    s[i] = i;
                }

                Callback cb = new Callback(lengths[l]);
                p.begin_opIntS(s).whenCompleted(
                    (int[] r) =>
                    {
                        cb.opIntS(r);
                    },
                    (Ice.Exception ex) =>
                    {
                         cb.exCB(ex);
                    });
                cb.check();
            }
        }

        {
            Dictionary<string, string> ctx = new Dictionary<string, string>();
            ctx["one"] = "ONE";
            ctx["two"] = "TWO";
            ctx["three"] = "THREE";
            {
                test(p.ice_getContext().Count == 0);
                Callback cb = new Callback(ctx);
                p.begin_opContext().whenCompleted(cb.opContextNotEqual, cb.exCB);
                cb.check();
            }
            {
                test(p.ice_getContext().Count == 0);
                Callback cb = new Callback(ctx);
                p.begin_opContext(ctx).whenCompleted(cb.opContextEqual, cb.exCB);
                cb.check();
            }
            {
                Test.MyClassPrx p2 = Test.MyClassPrxHelper.checkedCast(p.ice_context(ctx));
                test(Ice.CollectionComparer.Equals(p2.ice_getContext(), ctx));
                Callback cb = new Callback(ctx);
                p2.begin_opContext().whenCompleted(cb.opContextEqual, cb.exCB);
                cb.check();
            }
            {
                Test.MyClassPrx p2 = Test.MyClassPrxHelper.checkedCast(p.ice_context(ctx));
                Callback cb = new Callback(ctx);
                p2.begin_opContext(ctx).whenCompleted(cb.opContextEqual, cb.exCB);
                cb.check();
            }
        }

        {
            Dictionary<string, string> ctx = new Dictionary<string, string>();
            ctx["one"] = "ONE";
            ctx["two"] = "TWO";
            ctx["three"] = "THREE";
            {
                test(p.ice_getContext().Count == 0);
                Callback cb = new Callback(ctx);
                p.begin_opContext().whenCompleted(
                    (Dictionary<string, string> r) =>
                    {
                        cb.opContextNotEqual(r);
                    },
                    (Ice.Exception ex) =>
                    {
                         cb.exCB(ex);
                    });
                cb.check();
            }
            {
                test(p.ice_getContext().Count == 0);
                Callback cb = new Callback(ctx);
                p.begin_opContext(ctx).whenCompleted(
                    (Dictionary<string, string> r) =>
                    {
                        cb.opContextEqual(r);
                    },
                    (Ice.Exception ex) =>
                    {
                         cb.exCB(ex);
                    });
                cb.check();
            }
            {
                Test.MyClassPrx p2 = Test.MyClassPrxHelper.checkedCast(p.ice_context(ctx));
                test(Ice.CollectionComparer.Equals(p2.ice_getContext(), ctx));
                Callback cb = new Callback(ctx);
                p2.begin_opContext().whenCompleted(
                    (Dictionary<string, string> r) =>
                    {
                        cb.opContextEqual(r);
                    },
                    (Ice.Exception ex) =>
                    {
                         cb.exCB(ex);
                    });
                cb.check();
            }
            {
                Test.MyClassPrx p2 = Test.MyClassPrxHelper.checkedCast(p.ice_context(ctx));
                Callback cb = new Callback(ctx);
                p2.begin_opContext(ctx).whenCompleted(
                    (Dictionary<string, string> r) =>
                    {
                        cb.opContextEqual(r);
                    },
                    (Ice.Exception ex) =>
                    {
                         cb.exCB(ex);
                    });
                cb.check();
            }
        }

        if(p.ice_getConnection() != null)
        {
            //
            // Test implicit context propagation
            //

            string[] impls = {"Shared", "PerThread"};
            for(int i = 0; i < 2; i++)
            {
                Ice.InitializationData initData = new Ice.InitializationData();
                initData.properties = communicator.getProperties().ice_clone_();
                initData.properties.setProperty("Ice.ImplicitContext", impls[i]);

                Ice.Communicator ic = Ice.Util.initialize(initData);

                Dictionary<string, string> ctx = new Dictionary<string, string>();
                ctx["one"] = "ONE";
                ctx["two"] = "TWO";
                ctx["three"] = "THREE";

                Test.MyClassPrx p3 = Test.MyClassPrxHelper.uncheckedCast(
                                        ic.stringToProxy("test:default -p 12010"));

                ic.getImplicitContext().setContext(ctx);
                test(Ice.CollectionComparer.Equals(ic.getImplicitContext().getContext(), ctx));
                {
                    Ice.AsyncResult r = p3.begin_opContext();
                    Dictionary<string, string> c = p3.end_opContext(r);
                    test(Ice.CollectionComparer.Equals(c, ctx));
                }

                ic.getImplicitContext().put("zero", "ZERO");

                ctx = ic.getImplicitContext().getContext();
                {
                    Ice.AsyncResult r = p3.begin_opContext();
                    Dictionary<string, string> c = p3.end_opContext(r);
                    test(Ice.CollectionComparer.Equals(c, ctx));
                }

                Dictionary<string, string> prxContext = new Dictionary<string, string>();
                prxContext["one"] = "UN";
                prxContext["four"] = "QUATRE";

                Dictionary<string, string> combined = prxContext;
                foreach(KeyValuePair<string, string> e in ctx)
                {
                    try
                    {
                        combined.Add(e.Key, e.Value);
                    }
                    catch(System.ArgumentException)
                    {
                        // Ignore.
                    }
                }
                test(combined["one"].Equals("UN"));

                p3 = Test.MyClassPrxHelper.uncheckedCast(p.ice_context(prxContext));

                ic.getImplicitContext().setContext(null);
                {
                    Ice.AsyncResult r = p3.begin_opContext();
                    Dictionary<string, string> c = p3.end_opContext(r);
                    test(Ice.CollectionComparer.Equals(c, prxContext));
                }

                ic.getImplicitContext().setContext(ctx);
                {
                    Ice.AsyncResult r = p3.begin_opContext();
                    Dictionary<string, string> c = p3.end_opContext(r);
                    test(Ice.CollectionComparer.Equals(c, combined));
                }

                //ic.getImplicitContext().setContext(null);
                ic.destroy();
            }
        }

        {
            Ice.AsyncResult r = p.begin_opIdempotent();
            p.end_opIdempotent(r);
        }

        {
            Callback cb = new Callback();
            p.begin_opIdempotent().whenCompleted(cb.opIdempotent, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opIdempotent().whenCompleted(
                () =>
                {
                    cb.opIdempotent();
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Ice.AsyncResult r = p.begin_opNonmutating();
            p.end_opNonmutating(r);
        }

        {
            Callback cb = new Callback();
            p.begin_opNonmutating().whenCompleted(cb.opNonmutating, cb.exCB);
            cb.check();
        }

        {
            Callback cb = new Callback();
            p.begin_opNonmutating().whenCompleted(
                () =>
                {
                    cb.opNonmutating();
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }

        {
            Test.MyDerivedClassPrx derived = Test.MyDerivedClassPrxHelper.checkedCast(p);
            test(derived != null);
            Callback cb = new Callback();
            derived.begin_opDerived().whenCompleted(cb.opDerived, cb.exCB);
            cb.check();
        }

        {
            Test.MyDerivedClassPrx derived = Test.MyDerivedClassPrxHelper.checkedCast(p);
            test(derived != null);
            Callback cb = new Callback();
            derived.begin_opDerived().whenCompleted(
                () =>
                {
                    cb.opDerived();
                },
                (Ice.Exception ex) =>
                {
                     cb.exCB(ex);
                });
            cb.check();
        }
    }
}
