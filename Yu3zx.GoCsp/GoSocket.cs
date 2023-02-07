using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO.Ports;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;

namespace Yu3zx.GoCsp
{
    internal class TypeReflection
    {
        public static readonly Assembly _systemAss = Assembly.LoadFrom(RuntimeEnvironment.GetRuntimeDirectory() + "System.dll");
        public static readonly Assembly _mscorlibAss = Assembly.LoadFrom(RuntimeEnvironment.GetRuntimeDirectory() + "mscorlib.dll");

        public static MethodInfo GetMethod(Type type, string name, Type[] parmsType = null)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == name)
                {
                    if (null == parmsType)
                    {
                        return methods[i];
                    }
                    else
                    {
                        ParameterInfo[] parameters = methods[i].GetParameters();
                        if (parameters.Length == parmsType.Length)
                        {
                            int j = 0;
                            for (; j < parmsType.Length && parameters[j].ParameterType == parmsType[j]; j++) { }
                            if (j == parmsType.Length)
                            {
                                return methods[i];
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static FieldInfo GetField(Type type, string name)
        {
            FieldInfo[] members = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].Name == name)
                {
                    return members[i];
                }
            }
            return null;
        }
    }

    public struct SocketResult
    {
        public bool ok;
        public int s;
        public System.Exception ec;

        public SocketResult(bool resOk = false, int resSize = 0, System.Exception resEc = null)
        {
            ok = resOk;
            s = resSize;
            ec = resEc;
        }

        public string message
        {
            get
            {
                return null != ec ? ec.Message : null;
            }
        }

        static public implicit operator bool(SocketResult src)
        {
            return src.ok;
        }

        static public implicit operator int(SocketResult src)
        {
            return src.s;
        }
    }

    struct SocketSameHandler
    {
        public object pinnedObj;
        public Action<SocketResult> cb;
        public AsyncCallback handler;
    }

    struct SocketHandler
    {
        public int currTotal;
        public ArraySegment<byte> buff;
        public Action<SocketResult> cb;
        public Action<SocketResult> handler;
    }

    struct SocketPtrHandler
    {
        public int currTotal;
        public IntPtr ptr;
        public int offset;
        public int size;
        public object pinnedObj;
        public Action<SocketResult> handler;
        public Action<SocketResult> cb;
    }

    struct accept_handler
    {
        public GoSocketTcp sck;
        public AsyncCallback handler;
        public Action<SocketResult> cb;
    }

    struct accept_lost_handler
    {
        public GoSocketTcp sck;
        public Action<SocketResult> handler;
    }

    public abstract class GoSocket
    {
        [DllImport("kernel32.dll")]
        static extern int WriteFile(SafeHandle handle, IntPtr bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);
        [DllImport("kernel32.dll")]
        static extern int ReadFile(SafeHandle handle, IntPtr bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);
        [DllImport("kernel32.dll")]
        static extern int CancelIoEx(SafeHandle handle, IntPtr mustBeZero);

        static readonly Type _memoryAccessorType = TypeReflection._mscorlibAss.GetType("System.IO.UnmanagedMemoryAccessor");
        static readonly Type _memoryStreamType = TypeReflection._mscorlibAss.GetType("System.IO.UnmanagedMemoryStream");
        static readonly Type _safeHandleType = TypeReflection._mscorlibAss.GetType("System.Runtime.InteropServices.SafeHandle");
        static readonly FieldInfo _memoryAccessorSafeBuffer = TypeReflection.GetField(_memoryAccessorType, "_buffer");
        static readonly FieldInfo _memoryStreamSafeBuffer = TypeReflection.GetField(_memoryStreamType, "_buffer");
        static readonly FieldInfo _safeBufferHandle = TypeReflection.GetField(_safeHandleType, "handle");

        static public TupleEx<bool, int> SyncWrite(SafeHandle handle, IntPtr ptr, int offset, int size)
        {
            int num = 0;
            return new TupleEx<bool, int>(1 == WriteFile(handle, ptr + offset, size, out num, IntPtr.Zero), num);
        }

        static public TupleEx<bool, int> SyncRead(SafeHandle handle, IntPtr ptr, int offset, int size)
        {
            int num = 0;
            return new TupleEx<bool, int>(1 == ReadFile(handle, ptr + offset, size, out num, IntPtr.Zero), num);
        }

        static public TupleEx<bool, int> SyncWrite(SafeHandle handle, byte[] bytes, int offset, int size)
        {
            return SyncWrite(handle, Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0), offset, size);
        }

        static public TupleEx<bool, int> SyncRead(SafeHandle handle, byte[] bytes, int offset, int size)
        {
            return SyncRead(handle, Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0), offset, size);
        }

        static public TupleEx<bool, int> SyncWrite(SafeHandle handle, byte[] bytes)
        {
            return SyncWrite(handle, bytes, 0, bytes.Length);
        }

        static public TupleEx<bool, int> SyncRead(SafeHandle handle, byte[] bytes)
        {
            return SyncRead(handle, bytes, 0, bytes.Length);
        }

        static public bool cancel_io(SafeHandle handle)
        {
            return 1 == CancelIoEx(handle, IntPtr.Zero);
        }

        static public IntPtr get_mmview_ptr(MemoryMappedViewAccessor mmv)
        {
            return (IntPtr)_safeBufferHandle.GetValue(_memoryAccessorSafeBuffer.GetValue(mmv));
        }

        static public IntPtr get_mmview_ptr(MemoryMappedViewStream mmv)
        {
            return (IntPtr)_safeBufferHandle.GetValue(_memoryStreamSafeBuffer.GetValue(mmv));
        }

        SocketHandler _readHandler;
        SocketHandler _writeHandler;

        protected GoSocket()
        {
            _readHandler.cb = null;
            _readHandler.handler = delegate (SocketResult tempRes)
            {
                if (tempRes.ok)
                {
                    _readHandler.currTotal += tempRes.s;
                    _readHandler.buff = new ArraySegment<byte>(_readHandler.buff.Array, _readHandler.buff.Offset + tempRes.s, _readHandler.buff.Count - tempRes.s);
                    if (0 == _readHandler.buff.Count)
                    {
                        Action<SocketResult> cb = _readHandler.cb;
                        _readHandler.buff = default(ArraySegment<byte>);
                        _readHandler.cb = null;
                        Functional.CatchInvoke(cb, new SocketResult(true, _readHandler.currTotal));
                    }
                    else
                    {
                        async_read_same(_readHandler.buff, _readHandler.handler);
                    }
                }
                else
                {
                    Action<SocketResult> cb = _readHandler.cb;
                    _readHandler.buff = default(ArraySegment<byte>);
                    _readHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, _readHandler.currTotal, tempRes.ec));
                }
            };
            _writeHandler.cb = null;
            _writeHandler.handler = delegate (SocketResult tempRes)
            {
                if (tempRes.ok)
                {
                    _writeHandler.currTotal += tempRes.s;
                    _writeHandler.buff = new ArraySegment<byte>(_writeHandler.buff.Array, _writeHandler.buff.Offset + tempRes.s, _writeHandler.buff.Count - tempRes.s);
                    if (0 == _writeHandler.buff.Count)
                    {
                        Action<SocketResult> cb = _writeHandler.cb;
                        _writeHandler.buff = default(ArraySegment<byte>);
                        _writeHandler.cb = null;
                        Functional.CatchInvoke(cb, new SocketResult(true, _writeHandler.currTotal));
                    }
                    else
                    {
                        async_write_same(_writeHandler.buff, _writeHandler.handler);
                    }
                }
                else
                {
                    Action<SocketResult> cb = _writeHandler.cb;
                    _writeHandler.buff = default(ArraySegment<byte>);
                    _writeHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, _writeHandler.currTotal, tempRes.ec));
                }
            };
        }

        public abstract void async_read_same(ArraySegment<byte> buff, Action<SocketResult> cb);
        public abstract void async_write_same(ArraySegment<byte> buff, Action<SocketResult> cb);
        public abstract void Close();

        public void async_read_same(byte[] buff, Action<SocketResult> cb)
        {
            async_read_same(new ArraySegment<byte>(buff), cb);
        }

        public void async_write_same(byte[] buff, Action<SocketResult> cb)
        {
            async_write_same(new ArraySegment<byte>(buff), cb);
        }

        void _async_reads(int currTotal, int currIndex, IList<ArraySegment<byte>> buff, Action<SocketResult> cb)
        {
            if (buff.Count == currIndex)
            {
                Functional.CatchInvoke(cb, new SocketResult(true, currTotal));
            }
            else
            {
                async_read(buff[currIndex], delegate (SocketResult tempRes)
                {
                    if (tempRes.ok)
                    {
                        _async_reads(currTotal + tempRes.s, currIndex + 1, buff, cb);
                    }
                    else
                    {
                        Functional.CatchInvoke(cb, new SocketResult(false, currTotal, tempRes.ec));
                    }
                });
            }
        }

        void _async_reads(int currTotal, int currIndex, IList<byte[]> buff, Action<SocketResult> cb)
        {
            if (buff.Count == currIndex)
            {
                Functional.CatchInvoke(cb, new SocketResult(true, currTotal));
            }
            else
            {
                async_read(buff[currIndex], delegate (SocketResult tempRes)
                {
                    if (tempRes.ok)
                    {
                        _async_reads(currTotal + tempRes.s, currIndex + 1, buff, cb);
                    }
                    else
                    {
                        Functional.CatchInvoke(cb, new SocketResult(false, currTotal, tempRes.ec));
                    }
                });
            }
        }

        public void async_read(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            _readHandler.currTotal = 0;
            _readHandler.buff = buff;
            _readHandler.cb = cb;
            async_read_same(_readHandler.buff, _readHandler.handler);
        }

        public void async_read(byte[] buff, Action<SocketResult> cb)
        {
            async_read(new ArraySegment<byte>(buff), cb);
        }

        void _async_writes(int currTotal, int currIndex, IList<ArraySegment<byte>> buff, Action<SocketResult> cb)
        {
            if (buff.Count == currIndex)
            {
                Functional.CatchInvoke(cb, new SocketResult(true, currTotal));
            }
            else
            {
                async_write(buff[currIndex], delegate (SocketResult tempRes)
                {
                    if (tempRes.ok)
                    {
                        _async_writes(currTotal + tempRes.s, currIndex + 1, buff, cb);
                    }
                    else
                    {
                        Functional.CatchInvoke(cb, new SocketResult(false, currTotal, tempRes.ec));
                    }
                });
            }
        }

        void _async_writes(int currTotal, int currIndex, IList<byte[]> buff, Action<SocketResult> cb)
        {
            if (buff.Count == currIndex)
            {
                Functional.CatchInvoke(cb, new SocketResult(true, currTotal));
            }
            else
            {
                async_write(buff[currIndex], delegate (SocketResult tempRes)
                {
                    if (tempRes.ok)
                    {
                        _async_writes(currTotal + tempRes.s, currIndex + 1, buff, cb);
                    }
                    else
                    {
                        Functional.CatchInvoke(cb, new SocketResult(false, currTotal, tempRes.ec));
                    }
                });
            }
        }

        public void async_write(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            _writeHandler.currTotal = 0;
            _writeHandler.buff = buff;
            _writeHandler.cb = cb;
            async_write_same(_writeHandler.buff, _writeHandler.handler);
        }

        public void async_write(byte[] buff, Action<SocketResult> cb)
        {
            async_write(new ArraySegment<byte>(buff), cb);
        }

        public ValueTask<SocketResult> read_same(ArraySegment<byte> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read_same(buff, cb));
        }

        public ValueTask<SocketResult> read_same(byte[] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read_same(buff, cb));
        }

        public ValueTask<SocketResult> read(ArraySegment<byte> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read(buff, cb));
        }

        public ValueTask<SocketResult> read(byte[] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read(buff, cb));
        }

        public ValueTask<SocketResult> reads(IList<byte[]> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public ValueTask<SocketResult> reads(IList<ArraySegment<byte>> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public ValueTask<SocketResult> reads(params byte[][] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public ValueTask<SocketResult> reads(params ArraySegment<byte>[] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public ValueTask<SocketResult> write_same(ArraySegment<byte> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write_same(buff, cb));
        }

        public ValueTask<SocketResult> write_same(byte[] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write_same(buff, cb));
        }

        public ValueTask<SocketResult> write(ArraySegment<byte> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write(buff, cb));
        }

        public ValueTask<SocketResult> write(byte[] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write(buff, cb));
        }

        public ValueTask<SocketResult> writes(IList<byte[]> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }

        public ValueTask<SocketResult> writes(IList<ArraySegment<byte>> buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }

        public ValueTask<SocketResult> writes(params byte[][] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }

        public ValueTask<SocketResult> writes(params ArraySegment<byte>[] buff)
        {
            return Generator.async_call((Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }

        public Task unsafe_read_same(AsyncResultWrap<SocketResult> res, ArraySegment<byte> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read_same(buff, cb));
        }

        public Task unsafe_read_same(AsyncResultWrap<SocketResult> res, byte[] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read_same(buff, cb));
        }

        public Task unsafe_read(AsyncResultWrap<SocketResult> res, ArraySegment<byte> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read(buff, cb));
        }

        public Task unsafe_read(AsyncResultWrap<SocketResult> res, byte[] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read(buff, cb));
        }

        public Task unsafe_reads(AsyncResultWrap<SocketResult> res, IList<byte[]> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public Task unsafe_reads(AsyncResultWrap<SocketResult> res, IList<ArraySegment<byte>> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public Task unsafe_reads(AsyncResultWrap<SocketResult> res, params byte[][] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public Task unsafe_reads(AsyncResultWrap<SocketResult> res, params ArraySegment<byte>[] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_reads(0, 0, buff, cb));
        }

        public Task unsafe_write_same(AsyncResultWrap<SocketResult> res, ArraySegment<byte> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write_same(buff, cb));
        }

        public Task unsafe_write_same(AsyncResultWrap<SocketResult> res, byte[] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write_same(buff, cb));
        }

        public Task unsafe_write(AsyncResultWrap<SocketResult> res, ArraySegment<byte> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write(buff, cb));
        }

        public Task unsafe_write(AsyncResultWrap<SocketResult> res, byte[] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write(buff, cb));
        }

        public Task unsafe_writes(AsyncResultWrap<SocketResult> res, IList<byte[]> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }

        public Task unsafe_writes(AsyncResultWrap<SocketResult> res, IList<ArraySegment<byte>> buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }

        public Task unsafe_writes(AsyncResultWrap<SocketResult> res, params byte[][] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }

        public Task unsafe_writes(AsyncResultWrap<SocketResult> res, params ArraySegment<byte>[] buff)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => _async_writes(0, 0, buff, cb));
        }
    }

    public class GoSocketTcp : GoSocket
    {
        class SocketAsyncIo
        {
            [DllImport("Ws2_32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
            static extern SocketError WSASend(IntPtr socketHandle, IntPtr buffer, int bufferCount, out int bytesTransferred, SocketFlags socketFlags, SafeHandle overlapped, IntPtr completionRoutine);
            [DllImport("Ws2_32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
            static extern SocketError WSARecv(IntPtr socketHandle, IntPtr buffer, int bufferCount, out int bytesTransferred, ref SocketFlags socketFlags, SafeHandle overlapped, IntPtr completionRoutine);
            [DllImport("Kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
            static extern int SetFilePointerEx(SafeHandle fileHandle, long liDistanceToMove, out long lpNewFilePointer, int dwMoveMethod);
            [DllImport("Kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
            static extern int GetFileSizeEx(SafeHandle fileHandle, out long lpFileSize);
            [DllImport("Mswsock.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
            static extern int TransmitFile(IntPtr socketHandle, SafeHandle fileHandle, int nNumberOfBytesToWrite, int nNumberOfBytesPerSend, SafeHandle overlapped, IntPtr mustBeZero, int dwFlags);

            static readonly Type _overlappedType = TypeReflection._systemAss.GetType("System.Net.Sockets.OverlappedAsyncResult");
            static readonly Type _cacheSetType = TypeReflection._systemAss.GetType("System.Net.Sockets.Socket+CacheSet");
            static readonly Type _callbackClosureType = TypeReflection._systemAss.GetType("System.Net.CallbackClosure");
            static readonly Type _callbackClosureRefType = TypeReflection._systemAss.GetType("System.Net.CallbackClosure&");
            static readonly Type _overlappedCacheRefType = TypeReflection._systemAss.GetType("System.Net.Sockets.OverlappedCache&");
            static readonly Type _WSABufferType = TypeReflection._systemAss.GetType("System.Net.WSABuffer");
            static readonly MethodInfo _overlappedStartPostingAsyncOp = TypeReflection.GetMethod(_overlappedType, "StartPostingAsyncOp", new Type[] { typeof(bool) });
            static readonly MethodInfo _overlappedFinishPostingAsyncOp = TypeReflection.GetMethod(_overlappedType, "FinishPostingAsyncOp", new Type[] { _callbackClosureRefType });
            static readonly MethodInfo _overlappedSetupCache = TypeReflection.GetMethod(_overlappedType, "SetupCache", new Type[] { _overlappedCacheRefType });
            static readonly MethodInfo _overlappedExtractCache = TypeReflection.GetMethod(_overlappedType, "ExtractCache", new Type[] { _overlappedCacheRefType });
            static readonly MethodInfo _overlappedSetUnmanagedStructures = TypeReflection.GetMethod(_overlappedType, "SetUnmanagedStructures", new Type[] { typeof(object) });
            static readonly MethodInfo _overlappedOverlappedHandle = TypeReflection.GetMethod(_overlappedType, "get_OverlappedHandle");
            static readonly MethodInfo _overlappedCheckAsyncCallOverlappedResult = TypeReflection.GetMethod(_overlappedType, "CheckAsyncCallOverlappedResult", new Type[] { typeof(SocketError) });
            static readonly MethodInfo _overlappedInvokeCallback = TypeReflection.GetMethod(_overlappedType, "InvokeCallback", new Type[] { typeof(object) });
            static readonly MethodInfo _socketCaches = TypeReflection.GetMethod(typeof(Socket), "get_Caches");
            static readonly MethodInfo _sockektUpdateStatusAfterSocketError = TypeReflection.GetMethod(typeof(Socket), "UpdateStatusAfterSocketError", new Type[] { typeof(SocketError) });
            static readonly FieldInfo _overlappedmSingleBuffer = TypeReflection.GetField(_overlappedType, "m_SingleBuffer");
            static readonly FieldInfo _WSABufferLength = TypeReflection.GetField(_WSABufferType, "Length");
            static readonly FieldInfo _WSABufferPointer = TypeReflection.GetField(_WSABufferType, "Pointer");
            static readonly FieldInfo _cacheSendClosureCache = TypeReflection.GetField(_cacheSetType, "SendClosureCache");
            static readonly FieldInfo _cacheSendOverlappedCache = TypeReflection.GetField(_cacheSetType, "SendOverlappedCache");
            static readonly FieldInfo _cacheReceiveClosureCache = TypeReflection.GetField(_cacheSetType, "ReceiveClosureCache");
            static readonly FieldInfo _cacheReceiveOverlappedCache = TypeReflection.GetField(_cacheSetType, "ReceiveOverlappedCache");

            static readonly string _overlappedTypeName = _overlappedType.FullName;
            static private IAsyncResult MakeOverlappedAsyncResult(Socket sck, AsyncCallback callback)
            {
                return (IAsyncResult)TypeReflection._systemAss.CreateInstance(_overlappedTypeName, true,
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { sck, null, callback }, null, null);
            }

            static readonly object[] startPostingAsyncOpParam = new object[] { false };
            static private void StartPostingAsyncOp(IAsyncResult overlapped)
            {
                _overlappedStartPostingAsyncOp.Invoke(overlapped, startPostingAsyncOpParam);
            }

            static private void FinishPostingAsyncSendOp(IAsyncResult overlapped, object cacheSet)
            {
                object oldClosure = _cacheSendClosureCache.GetValue(cacheSet);
                object[] closure = new object[] { oldClosure };
                _overlappedFinishPostingAsyncOp.Invoke(overlapped, closure);
                if (oldClosure != closure[0])
                {
                    _cacheSendClosureCache.SetValue(cacheSet, closure[0]);
                }
            }

            static private void FinishPostingAsyncRecvOp(IAsyncResult overlapped, object cacheSet)
            {
                object oldClosure = _cacheReceiveClosureCache.GetValue(cacheSet);
                object[] closure = new object[] { oldClosure };
                _overlappedFinishPostingAsyncOp.Invoke(overlapped, closure);
                if (oldClosure != closure[0])
                {
                    _cacheReceiveClosureCache.SetValue(cacheSet, closure[0]);
                }
            }

            static readonly object[] nullPinnedObj = new object[] { null };
            static private void SetUnmanagedStructures(IAsyncResult overlapped, object pinnedObj)
            {
                _overlappedSetUnmanagedStructures.Invoke(overlapped, null == pinnedObj ? nullPinnedObj : new object[] { pinnedObj });
            }

            static private IntPtr SetSendPointer(IAsyncResult overlapped, IntPtr ptr, int offset, int size, object cacheSet, object pinnedObj)
            {
                object oldCache = _cacheSendOverlappedCache.GetValue(cacheSet);
                object[] overlappedCache = new object[] { oldCache };
                _overlappedSetupCache.Invoke(overlapped, overlappedCache);
                if (oldCache != overlappedCache[0])
                {
                    _cacheSendOverlappedCache.SetValue(cacheSet, overlappedCache[0]);
                }
                SetUnmanagedStructures(overlapped, pinnedObj);
                object WSABuffer = _overlappedmSingleBuffer.GetValue(overlapped);
                _WSABufferPointer.SetValue(WSABuffer, ptr + offset);
                _WSABufferLength.SetValue(WSABuffer, size);
                _overlappedmSingleBuffer.SetValue(overlapped, WSABuffer);
                return GCHandle.Alloc(WSABuffer, GCHandleType.Pinned).AddrOfPinnedObject();
            }

            static private IntPtr SetRecvPointer(IAsyncResult overlapped, IntPtr ptr, int offset, int size, object cacheSet, object pinnedObj)
            {
                object oldCache = _cacheReceiveOverlappedCache.GetValue(cacheSet);
                object[] overlappedCache = new object[] { oldCache };
                _overlappedSetupCache.Invoke(overlapped, overlappedCache);
                if (oldCache != overlappedCache[0])
                {
                    _cacheReceiveOverlappedCache.SetValue(cacheSet, overlappedCache[0]);
                }
                SetUnmanagedStructures(overlapped, pinnedObj);
                object WSABuffer = _overlappedmSingleBuffer.GetValue(overlapped);
                _WSABufferPointer.SetValue(WSABuffer, ptr + offset);
                _WSABufferLength.SetValue(WSABuffer, size);
                _overlappedmSingleBuffer.SetValue(overlapped, WSABuffer);
                return GCHandle.Alloc(WSABuffer, GCHandleType.Pinned).AddrOfPinnedObject();
            }

            static public SocketError Send(Socket sck, IntPtr ptr, int offset, int size, SocketFlags socketFlags, AsyncCallback callback)
            {
                IAsyncResult overlapped = MakeOverlappedAsyncResult(sck, callback);
                StartPostingAsyncOp(overlapped);
                object cacheSet = _socketCaches.Invoke(sck, null);
                IntPtr WSABuffer = SetSendPointer(overlapped, ptr, offset, size, cacheSet, null);
                int num = 0;
                SafeHandle overlappedHandle = (SafeHandle)_overlappedOverlappedHandle.Invoke(overlapped, null);
                SocketError lastWin32Error = WSASend(sck.Handle, WSABuffer, 1, out num, socketFlags, overlappedHandle, IntPtr.Zero);
                if (SocketError.Success != lastWin32Error)
                {
                    lastWin32Error = (SocketError)Marshal.GetLastWin32Error();
                }
                if (SocketError.Success == lastWin32Error || SocketError.IOPending == lastWin32Error)
                {
                    FinishPostingAsyncSendOp(overlapped, cacheSet);
                    return SocketError.Success;
                }
                lastWin32Error = (SocketError)_overlappedCheckAsyncCallOverlappedResult.Invoke(overlapped, new object[] { lastWin32Error });
                object oldCache = _cacheSendOverlappedCache.GetValue(cacheSet);
                object[] overlappedCache = new object[] { oldCache };
                _overlappedExtractCache.Invoke(overlapped, overlappedCache);
                if (oldCache != overlappedCache[0])
                {
                    _cacheSendOverlappedCache.SetValue(cacheSet, overlappedCache[0]);
                }
                _sockektUpdateStatusAfterSocketError.Invoke(sck, new object[] { lastWin32Error });
                return lastWin32Error;
            }

            static public SocketError Recv(Socket sck, IntPtr ptr, int offset, int size, SocketFlags socketFlags, AsyncCallback callback)
            {
                IAsyncResult overlapped = MakeOverlappedAsyncResult(sck, callback);
                StartPostingAsyncOp(overlapped);
                object cacheSet = _socketCaches.Invoke(sck, null);
                IntPtr WSABuffer = SetRecvPointer(overlapped, ptr, offset, size, cacheSet, null);
                int num = 0;
                SafeHandle overlappedHandle = (SafeHandle)_overlappedOverlappedHandle.Invoke(overlapped, null);
                SocketError lastWin32Error = WSARecv(sck.Handle, WSABuffer, 1, out num, ref socketFlags, overlappedHandle, IntPtr.Zero);
                if (SocketError.Success != lastWin32Error)
                {
                    lastWin32Error = (SocketError)Marshal.GetLastWin32Error();
                }
                if (SocketError.Success == lastWin32Error || SocketError.IOPending == lastWin32Error)
                {
                    FinishPostingAsyncRecvOp(overlapped, cacheSet);
                    return SocketError.Success;
                }
                lastWin32Error = (SocketError)_overlappedCheckAsyncCallOverlappedResult.Invoke(overlapped, new object[] { lastWin32Error });
                object oldCache = _cacheReceiveOverlappedCache.GetValue(cacheSet);
                object[] overlappedCache = new object[] { oldCache };
                _overlappedExtractCache.Invoke(overlapped, overlappedCache);
                if (oldCache != overlappedCache[0])
                {
                    _cacheReceiveOverlappedCache.SetValue(cacheSet, overlappedCache[0]);
                }
                _sockektUpdateStatusAfterSocketError.Invoke(sck, new object[] { lastWin32Error });
                _overlappedInvokeCallback.Invoke(overlapped, new object[] { new SocketException((int)lastWin32Error) });
                return lastWin32Error;
            }

            static public SocketError SendFile(Socket sck, SafeHandle fileHandle, long offset, int size, AsyncCallback callback)
            {
                IAsyncResult overlapped = MakeOverlappedAsyncResult(sck, callback);
                StartPostingAsyncOp(overlapped);
                object cacheSet = _socketCaches.Invoke(sck, null);
                SocketError lastWin32Error = SocketError.SocketError;
                do
                {
                    if (offset >= 0)
                    {
                        long newOffset = 0;
                        if (0 == SetFilePointerEx(fileHandle, offset, out newOffset, 0) || offset != newOffset)
                        {
                            break;
                        }
                    }
                    lastWin32Error = SocketError.Success;
                    SetSendPointer(overlapped, IntPtr.Zero, 0, 0, cacheSet, null);
                    SafeHandle overlappedHandle = (SafeHandle)_overlappedOverlappedHandle.Invoke(overlapped, null);
                    if (0 == TransmitFile(sck.Handle, fileHandle, size, 0, overlappedHandle, IntPtr.Zero, 0x20 | 0x04))
                    {
                        lastWin32Error = (SocketError)Marshal.GetLastWin32Error();
                    }
                    if (SocketError.Success == lastWin32Error || SocketError.IOPending == lastWin32Error)
                    {
                        FinishPostingAsyncSendOp(overlapped, cacheSet);
                        return SocketError.Success;
                    }
                } while (false);
                lastWin32Error = (SocketError)_overlappedCheckAsyncCallOverlappedResult.Invoke(overlapped, new object[] { lastWin32Error });
                object oldCache = _cacheSendOverlappedCache.GetValue(cacheSet);
                object[] overlappedCache = new object[] { oldCache };
                _overlappedExtractCache.Invoke(overlapped, overlappedCache);
                if (oldCache != overlappedCache[0])
                {
                    _cacheSendOverlappedCache.SetValue(cacheSet, overlappedCache[0]);
                }
                _sockektUpdateStatusAfterSocketError.Invoke(sck, new object[] { lastWin32Error });
                return lastWin32Error;
            }
        }

        Socket _socket;
        EndPoint _localPoint;
        SocketSameHandler _readSameHandler;
        SocketSameHandler _writeSameHandler;
        SocketPtrHandler _readPtrHandler;
        SocketPtrHandler _writePtrHandler;

        public GoSocketTcp()
        {
            _readSameHandler.pinnedObj = null;
            _readSameHandler.cb = null;
            _readSameHandler.handler = delegate (IAsyncResult ar)
            {
                object pinnedObj = _readSameHandler.pinnedObj;
                Action<SocketResult> cb = _readSameHandler.cb;
                _readSameHandler.pinnedObj = null;
                _readSameHandler.cb = null;
                try
                {
                    int s = _socket.EndReceive(ar);
                    Functional.CatchInvoke(cb, new SocketResult(0 != s, s));
                }
                catch (System.Exception ec)
                {
                    Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                }
            };
            _writeSameHandler.pinnedObj = null;
            _writeSameHandler.cb = null;
            _writeSameHandler.handler = delegate (IAsyncResult ar)
            {
                object pinnedObj = _writeSameHandler.pinnedObj;
                Action<SocketResult> cb = _writeSameHandler.cb;
                _writeSameHandler.pinnedObj = null;
                _writeSameHandler.cb = null;
                try
                {
                    int s = _socket.EndSend(ar);
                    Functional.CatchInvoke(cb, new SocketResult(0 != s, s));
                }
                catch (System.Exception ec)
                {
                    Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                }
            };
            _readPtrHandler.pinnedObj = null;
            _readPtrHandler.cb = null;
            _readPtrHandler.handler = delegate (SocketResult tempRes)
            {
                if (tempRes.ok)
                {
                    _readPtrHandler.currTotal += tempRes.s;
                    _readPtrHandler.offset += tempRes.s;
                    _readPtrHandler.size -= tempRes.s;
                    if (0 == _readPtrHandler.size)
                    {
                        object pinnedObj = _readPtrHandler.pinnedObj;
                        Action<SocketResult> cb = _readPtrHandler.cb;
                        _readPtrHandler.pinnedObj = null;
                        _readPtrHandler.cb = null;
                        Functional.CatchInvoke(cb, new SocketResult(true, _readPtrHandler.currTotal));
                    }
                    else
                    {
                        async_read_same(_readPtrHandler.ptr, _readPtrHandler.offset, _readPtrHandler.size, _readPtrHandler.handler);
                    }
                }
                else
                {
                    object pinnedObj = _readPtrHandler.pinnedObj;
                    Action<SocketResult> cb = _readPtrHandler.cb;
                    _readPtrHandler.pinnedObj = null;
                    _readPtrHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, _readPtrHandler.currTotal, tempRes.ec));
                }
            };
            _writePtrHandler.pinnedObj = null;
            _writePtrHandler.cb = null;
            _writePtrHandler.handler = delegate (SocketResult tempRes)
            {
                if (tempRes.ok)
                {
                    _writePtrHandler.currTotal += tempRes.s;
                    _writePtrHandler.offset += tempRes.s;
                    _writePtrHandler.size -= tempRes.s;
                    if (0 == _writePtrHandler.size)
                    {
                        object pinnedObj = _writePtrHandler.pinnedObj;
                        Action<SocketResult> cb = _writePtrHandler.cb;
                        _writePtrHandler.pinnedObj = null;
                        _writePtrHandler.cb = null;
                        Functional.CatchInvoke(cb, new SocketResult(true, _writePtrHandler.currTotal));
                    }
                    else
                    {
                        async_write_same(_writePtrHandler.ptr, _writePtrHandler.offset, _writePtrHandler.size, _writePtrHandler.handler);
                    }
                }
                else
                {
                    object pinnedObj = _writePtrHandler.pinnedObj;
                    Action<SocketResult> cb = _writePtrHandler.cb;
                    _writePtrHandler.pinnedObj = null;
                    _writePtrHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, _writePtrHandler.currTotal, tempRes.ec));
                }
            };
        }

        public Socket GoSocket
        {
            get
            {
                return _socket;
            }
        }

        public override void Close()
        {
            try
            {
                _socket?.Close();
            }
            catch (System.Exception) { }
        }

        public void Bind(string ip)
        {
            _localPoint = null == ip ? null : new IPEndPoint(IPAddress.Parse(ip), 0);
        }

        public void async_connect(string ip, int port, Action<SocketResult> cb)
        {
            try
            {
                if (null != _socket)
                {
                    Functional.CatchInvoke(cb, new SocketResult(false));
                    return;
                }
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (null != _localPoint)
                {
                    _socket.Bind(_localPoint);
                }
                _socket.BeginConnect(IPAddress.Parse(ip), port, delegate (IAsyncResult ar)
                {
                    try
                    {
                        _socket.EndConnect(ar);
                        _socket.NoDelay = true;
                        Functional.CatchInvoke(cb, new SocketResult(true));
                    }
                    catch (System.Exception ec)
                    {
                        Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                    }
                }, null);
            }
            catch (System.Exception ec)
            {
                Close();
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public void async_disconnect(bool reuseSocket, Action<SocketResult> cb)
        {
            try
            {
                _socket.BeginDisconnect(reuseSocket, delegate (IAsyncResult ar)
                {
                    try
                    {
                        _socket.EndDisconnect(ar);
                        Functional.CatchInvoke(cb, new SocketResult(true));
                    }
                    catch (System.Exception ec)
                    {
                        Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                    }
                }, null);
            }
            catch (System.Exception ec)
            {
                Close();
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public override void async_read_same(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            try
            {
                Debug.Assert(null == _readSameHandler.cb, "重入的 read 操作!");
                _readSameHandler.cb = cb;
                _socket.BeginReceive(buff.Array, buff.Offset, buff.Count, 0, _readSameHandler.handler, null);
            }
            catch (System.Exception ec)
            {
                Close();
                _readSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public override void async_write_same(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            try
            {
                Debug.Assert(null == _writeSameHandler.cb, "重入的 write 操作!");
                _writeSameHandler.cb = cb;
                _socket.BeginSend(buff.Array, buff.Offset, buff.Count, 0, _writeSameHandler.handler, null);
            }
            catch (System.Exception ec)
            {
                Close();
                _writeSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public void async_send_file(SafeHandle fileHandle, long offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            try
            {
                Debug.Assert(null == _writeSameHandler.cb, "重入的 write 操作!");
                _writeSameHandler.pinnedObj = pinnedObj;
                _writeSameHandler.cb = cb;
                SocketError lastWin32Error = SocketAsyncIo.SendFile(_socket, fileHandle, offset, size, _writeSameHandler.handler);
                if (SocketError.Success != lastWin32Error)
                {
                    Close();
                    _writeSameHandler.pinnedObj = null;
                    _writeSameHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, (int)lastWin32Error));
                }
            }
            catch (System.Exception ec)
            {
                Close();
                _writeSameHandler.pinnedObj = null;
                _writeSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public void async_read_same(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            try
            {
                Debug.Assert(null == _readSameHandler.cb, "重入的 read 操作!");
                _readSameHandler.pinnedObj = pinnedObj;
                _readSameHandler.cb = cb;
                SocketError lastWin32Error = SocketAsyncIo.Recv(_socket, ptr, offset, size, 0, _readSameHandler.handler);
                if (SocketError.Success != lastWin32Error)
                {
                    Close();
                    _readSameHandler.pinnedObj = null;
                    _readSameHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, (int)lastWin32Error));
                }
            }
            catch (System.Exception ec)
            {
                Close();
                _readSameHandler.pinnedObj = null;
                _readSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public void async_write_same(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            try
            {
                Debug.Assert(null == _writeSameHandler.cb, "重入的 write 操作!");
                _writeSameHandler.pinnedObj = pinnedObj;
                _writeSameHandler.cb = cb;
                SocketError lastWin32Error = SocketAsyncIo.Send(_socket, ptr, offset, size, 0, _writeSameHandler.handler);
                if (SocketError.Success != lastWin32Error)
                {
                    Close();
                    _writeSameHandler.pinnedObj = null;
                    _writeSameHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, (int)lastWin32Error));
                }
            }
            catch (System.Exception ec)
            {
                Close();
                _writeSameHandler.pinnedObj = null;
                _writeSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public void async_read(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            _readPtrHandler.currTotal = 0;
            _readPtrHandler.ptr = ptr;
            _readPtrHandler.offset = offset;
            _readPtrHandler.size = size;
            _readPtrHandler.pinnedObj = pinnedObj;
            async_read_same(_readPtrHandler.ptr, _readPtrHandler.offset, _readPtrHandler.size, _readPtrHandler.handler);
        }

        public void async_write(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            _writePtrHandler.currTotal = 0;
            _writePtrHandler.ptr = ptr;
            _writePtrHandler.offset = offset;
            _writePtrHandler.size = size;
            _writePtrHandler.pinnedObj = pinnedObj;
            async_write_same(_writePtrHandler.ptr, _writePtrHandler.offset, _writePtrHandler.size, _writePtrHandler.handler);
        }

        public ValueTask<SocketResult> read_same(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read_same(ptr, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> write_same(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write_same(ptr, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> read(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read(ptr, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> write(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write(ptr, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> send_file(SafeHandle fileHandle, long offset = 0, int size = 0, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_send_file(fileHandle, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> send_file(System.IO.FileStream file)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_send_file(file.SafeFileHandle, -1, 0, cb, file));
        }

        public ValueTask<SocketResult> connect(string ip, int port)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_connect(ip, port, cb));
        }

        public ValueTask<SocketResult> disconnect(bool reuseSocket)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_disconnect(reuseSocket, cb));
        }

        public Task unsafe_read_same(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read_same(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_write_same(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write_same(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_read(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_write(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_send_file(AsyncResultWrap<SocketResult> res, SafeHandle fileHandle, long offset = 0, int size = 0, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_send_file(fileHandle, offset, size, cb, pinnedObj));
        }

        public Task unsafe_send_file(AsyncResultWrap<SocketResult> res, System.IO.FileStream file)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_send_file(file.SafeFileHandle, -1, 0, cb, file));
        }

        public Task unsafe_connect(AsyncResultWrap<SocketResult> res, string ip, int port)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_connect(ip, port, cb));
        }

        public Task unsafe_disconnect(AsyncResultWrap<SocketResult> res, bool reuseSocket)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_disconnect(reuseSocket, cb));
        }

        public class acceptor
        {
            Socket _socket;
            accept_handler _acceptHandler;
            accept_lost_handler _lostHandler;

            public acceptor()
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _acceptHandler.sck = null;
                _acceptHandler.cb = null;
                _acceptHandler.handler = delegate (IAsyncResult ar)
                {
                    GoSocketTcp sck = _acceptHandler.sck;
                    Action<SocketResult> cb = _acceptHandler.cb;
                    _acceptHandler.sck = null;
                    _acceptHandler.cb = null;
                    try
                    {
                        sck._socket = _socket.EndAccept(ar);
                        sck._socket.NoDelay = true;
                        Functional.CatchInvoke(cb, new SocketResult(true));
                    }
                    catch (System.Exception ec)
                    {
                        Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                    }
                };
                _lostHandler.handler = delegate (SocketResult res)
                {
                    _lostHandler.sck?.Close();
                    _lostHandler.sck = null;
                };
            }

            public Socket GoSocket
            {
                get
                {
                    return _socket;
                }
            }

            public bool resue
            {
                set
                {
                    try
                    {
                        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value);
                    }
                    catch (System.Exception) { }
                }
            }

            public bool Bind(string ip, int port, int backlog = 1)
            {
                try
                {
                    _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                    _socket.Listen(backlog);
                    return true;
                }
                catch (System.Exception) { }
                return false;
            }

            public void Close()
            {
                try
                {
                    _socket.Close();
                }
                catch (System.Exception) { }
            }

            public void async_accept(GoSocketTcp sck, Action<SocketResult> cb)
            {
                try
                {
                    sck.Close();
                    _acceptHandler.sck = sck;
                    _acceptHandler.cb = cb;
                    _socket.BeginAccept(_acceptHandler.handler, null);
                }
                catch (System.Exception ec)
                {
                    Close();
                    Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                }
            }

            public ValueTask<SocketResult> accept(GoSocketTcp sck)
            {
                _lostHandler.sck = sck;
                return Generator.async_call((Action<SocketResult> cb) => async_accept(sck, cb), _lostHandler.handler);
            }

            public Task unsafe_accept(AsyncResultWrap<SocketResult> res, GoSocketTcp sck)
            {
                return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_accept(sck, cb));
            }
        }
    }

    struct PipeSameHandler
    {
        public ArraySegment<byte> buff;
        public object pinnedObj;
        public Action<SocketResult> cb;
        public AsyncCallback handler;
    }

#if !NETCORE
    public class GoSocketSerial : GoSocket
    {
        SerialPort _socket;
        PipeSameHandler _readSameHandler;
        PipeSameHandler _writeSameHandler;

        public GoSocketSerial(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _socket = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _readSameHandler.pinnedObj = null;
            _readSameHandler.cb = null;
            _readSameHandler.handler = delegate (IAsyncResult ar)
            {
                Action<SocketResult> cb = _readSameHandler.cb;
                _readSameHandler.cb = null;
                try
                {
                    int s = _socket.BaseStream.EndRead(ar);
                    Functional.CatchInvoke(cb, new SocketResult(0 != s, s));
                }
                catch (System.Exception ec)
                {
                    Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                }
            };
            _writeSameHandler.pinnedObj = null;
            _writeSameHandler.cb = null;
            _writeSameHandler.handler = delegate (IAsyncResult ar)
            {
                ArraySegment<byte> buff = _writeSameHandler.buff;
                Action<SocketResult> cb = _writeSameHandler.cb;
                _writeSameHandler.buff = default(ArraySegment<byte>);
                _writeSameHandler.cb = null;
                try
                {
                    _socket.BaseStream.EndWrite(ar);
                    Functional.CatchInvoke(cb, new SocketResult(true, buff.Count));
                }
                catch (System.Exception ec)
                {
                    Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                }
            };
        }

        public SerialPort GoSocket
        {
            get
            {
                return _socket;
            }
        }

        public bool open()
        {
            try
            {
                _socket.Open();
                return true;
            }
            catch (System.Exception) { }
            return false;
        }

        public override void Close()
        {
            try
            {
                _socket.Close();
            }
            catch (System.Exception) { }
        }

        public override void async_read_same(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            try
            {
                Debug.Assert(null == _readSameHandler.cb, "重入的 read 操作!");
                _readSameHandler.cb = cb;
                _socket.BaseStream.BeginRead(buff.Array, buff.Offset, buff.Count, _readSameHandler.handler, null);
            }
            catch (System.Exception ec)
            {
                Close();
                _readSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public override void async_write_same(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            try
            {
                Debug.Assert(null == _writeSameHandler.cb, "重入的 write 操作!");
                _writeSameHandler.buff = buff;
                _writeSameHandler.cb = cb;
                _socket.BaseStream.BeginWrite(buff.Array, buff.Offset, buff.Count, _writeSameHandler.handler, null);
            }
            catch (System.Exception ec)
            {
                Close();
                _writeSameHandler.buff = default(ArraySegment<byte>);
                _writeSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public void clear_in_buffer()
        {
            try
            {
                _socket.DiscardInBuffer();
            }
            catch (System.Exception) { }
        }

        public void clear_out_buffer()
        {
            try
            {
                _socket.DiscardOutBuffer();
            }
            catch (System.Exception) { }
        }
    }
#endif

    public abstract class SocketPipe : GoSocket
    {
        protected PipeStream _socket;
        PipeSameHandler _readSameHandler;
        PipeSameHandler _writeSameHandler;
        SocketPtrHandler _readPtrHandler;
        SocketPtrHandler _writePtrHandler;

        protected SocketPipe()
        {
            _readSameHandler.pinnedObj = null;
            _readSameHandler.cb = null;
            _readSameHandler.handler = delegate (IAsyncResult ar)
            {
                Action<SocketResult> cb = _readSameHandler.cb;
                _readSameHandler.cb = null;
                try
                {
                    int s = _socket.EndRead(ar);
                    Functional.CatchInvoke(cb, new SocketResult(0 != s, s));
                }
                catch (System.Exception ec)
                {
                    Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                }
            };
            _writeSameHandler.pinnedObj = null;
            _writeSameHandler.cb = null;
            _writeSameHandler.handler = delegate (IAsyncResult ar)
            {
                ArraySegment<byte> buff = _writeSameHandler.buff;
                Action<SocketResult> cb = _writeSameHandler.cb;
                _writeSameHandler.buff = default(ArraySegment<byte>);
                _writeSameHandler.cb = null;
                try
                {
                    _socket.EndWrite(ar);
                    Functional.CatchInvoke(cb, new SocketResult(true, buff.Count));
                }
                catch (System.Exception ec)
                {
                    Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
                }
            };
            _readPtrHandler.pinnedObj = null;
            _readPtrHandler.cb = null;
            _readPtrHandler.handler = delegate (SocketResult tempRes)
            {
                if (tempRes.ok)
                {
                    _readPtrHandler.currTotal += tempRes.s;
                    _readPtrHandler.offset += tempRes.s;
                    _readPtrHandler.size -= tempRes.s;
                    if (0 == _readPtrHandler.size)
                    {
                        object pinnedObj = _readPtrHandler.pinnedObj;
                        Action<SocketResult> cb = _readPtrHandler.cb;
                        _readPtrHandler.pinnedObj = null;
                        _readPtrHandler.cb = null;
                        Functional.CatchInvoke(cb, new SocketResult(true, _readPtrHandler.currTotal));
                    }
                    else
                    {
                        async_read_same(_readPtrHandler.ptr, _readPtrHandler.offset, _readPtrHandler.size, _readPtrHandler.handler);
                    }
                }
                else
                {
                    object pinnedObj = _readPtrHandler.pinnedObj;
                    Action<SocketResult> cb = _readPtrHandler.cb;
                    _readPtrHandler.pinnedObj = null;
                    _readPtrHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, _readPtrHandler.currTotal, tempRes.ec));
                }
            };
            _writePtrHandler.pinnedObj = null;
            _writePtrHandler.cb = null;
            _writePtrHandler.handler = delegate (SocketResult tempRes)
            {
                if (tempRes.ok)
                {
                    _writePtrHandler.currTotal += tempRes.s;
                    _writePtrHandler.offset += tempRes.s;
                    _writePtrHandler.size -= tempRes.s;
                    if (0 == _writePtrHandler.size)
                    {
                        object pinnedObj = _writePtrHandler.pinnedObj;
                        Action<SocketResult> cb = _writePtrHandler.cb;
                        _writePtrHandler.pinnedObj = null;
                        _writePtrHandler.cb = null;
                        Functional.CatchInvoke(cb, new SocketResult(true, _writePtrHandler.currTotal));
                    }
                    else
                    {
                        async_write_same(_writePtrHandler.ptr, _writePtrHandler.offset, _writePtrHandler.size, _writePtrHandler.handler);
                    }
                }
                else
                {
                    object pinnedObj = _writePtrHandler.pinnedObj;
                    Action<SocketResult> cb = _writePtrHandler.cb;
                    _writePtrHandler.pinnedObj = null;
                    _writePtrHandler.cb = null;
                    Functional.CatchInvoke(cb, new SocketResult(false, _writePtrHandler.currTotal, tempRes.ec));
                }
            };
        }

        public override void async_read_same(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            try
            {
                Debug.Assert(null == _readSameHandler.cb, "重入的 read 操作!");
                _readSameHandler.cb = cb;
                _socket.BeginRead(buff.Array, buff.Offset, buff.Count, _readSameHandler.handler, null);
            }
            catch (System.Exception ec)
            {
                Close();
                _readSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public override void async_write_same(ArraySegment<byte> buff, Action<SocketResult> cb)
        {
            try
            {
                Debug.Assert(null == _writeSameHandler.cb, "重入的 write 操作!");
                _writeSameHandler.buff = buff;
                _writeSameHandler.cb = cb;
                _socket.BeginWrite(buff.Array, buff.Offset, buff.Count, _writeSameHandler.handler, null);
            }
            catch (System.Exception ec)
            {
                Close();
                _writeSameHandler.buff = default(ArraySegment<byte>);
                _writeSameHandler.cb = null;
                Functional.CatchInvoke(cb, new SocketResult(false, 0, ec));
            }
        }

        public void async_read_same(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            Task.Run(delegate ()
            {
                if (_socket.IsConnected)
                {
                    try
                    {
                        object holdPin = pinnedObj;
                        TupleEx<bool, int> res = SyncRead(_socket.SafePipeHandle, ptr, offset, size);
                        Functional.CatchInvoke(cb, new SocketResult(res.value1, res.value2));
                        return;
                    }
                    catch (System.Exception) { }
                }
                Functional.CatchInvoke(cb, new SocketResult(false, 0));
            });
        }

        public void async_write_same(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            Task.Run(delegate ()
            {
                if (_socket.IsConnected)
                {
                    try
                    {
                        object holdPin = pinnedObj;
                        TupleEx<bool, int> res = SyncWrite(_socket.SafePipeHandle, ptr, offset, size);
                        Functional.CatchInvoke(cb, new SocketResult(res.value1, res.value2));
                        return;
                    }
                    catch (System.Exception) { }
                }
                Functional.CatchInvoke(cb, new SocketResult(false, 0));
            });
        }

        public void async_read(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            _readPtrHandler.currTotal = 0;
            _readPtrHandler.ptr = ptr;
            _readPtrHandler.offset = offset;
            _readPtrHandler.size = size;
            _readPtrHandler.pinnedObj = pinnedObj;
            _readPtrHandler.cb = cb;
            async_read_same(_readPtrHandler.ptr, _readPtrHandler.offset, _readPtrHandler.size, _readPtrHandler.handler);
        }

        public void async_write(IntPtr ptr, int offset, int size, Action<SocketResult> cb, object pinnedObj = null)
        {
            _writePtrHandler.currTotal = 0;
            _writePtrHandler.ptr = ptr;
            _writePtrHandler.offset = offset;
            _writePtrHandler.size = size;
            _writePtrHandler.pinnedObj = pinnedObj;
            _writePtrHandler.cb = cb;
            async_write_same(_writePtrHandler.ptr, _writePtrHandler.offset, _writePtrHandler.size, _writePtrHandler.handler);
        }

        public ValueTask<SocketResult> read_same(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read_same(ptr, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> write_same(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write_same(ptr, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> read(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_read(ptr, offset, size, cb, pinnedObj));
        }

        public ValueTask<SocketResult> write(IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.async_call((Action<SocketResult> cb) => async_write(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_read_same(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read_same(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_write_same(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write_same(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_read(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_read(ptr, offset, size, cb, pinnedObj));
        }

        public Task unsafe_write(AsyncResultWrap<SocketResult> res, IntPtr ptr, int offset, int size, object pinnedObj = null)
        {
            return Generator.unsafe_async_call(res, (Action<SocketResult> cb) => async_write(ptr, offset, size, cb, pinnedObj));
        }
    }

    public class SocketPipeServer : SocketPipe
    {
        readonly string _pipeName;

        public SocketPipeServer(string pipeName, int inBufferSize = 4 * 1024, int outBufferSize = 4 * 1024)
        {
            _pipeName = pipeName;
            _socket = new NamedPipeServerStream("Yu3zxGo_" + pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, inBufferSize, outBufferSize);
        }

        public NamedPipeServerStream GoSocket
        {
            get
            {
                return (NamedPipeServerStream)_socket;
            }
        }

        public override void Close()
        {
            if (_socket.IsConnected)
            {
                try
                {
                    ((NamedPipeServerStream)_socket).Disconnect();
                }
                catch (System.Exception) { }
            }
            try
            {
                _socket.Close();
            }
            catch (System.Exception) { }
        }

        public async Task<bool> wait_connection(int ms = -1)
        {
            bool overtime = false;
            AsyncTimer waitTimeout = null;
            try
            {
                if (ms >= 0)
                {
                    waitTimeout = new AsyncTimer(Generator.SelfStrand());
                    waitTimeout.Timeout(ms, delegate ()
                    {
                        overtime = true;
                        try
                        {
                            NamedPipeClientStream timedPipe = new NamedPipeClientStream(".", "Yu3zxGo_" + _pipeName);
                            timedPipe.Connect(0);
                            timedPipe.Close();
                        }
                        catch (System.Exception) { }
                    });
                }
                await Generator.send_task(((NamedPipeServerStream)_socket).WaitForConnection);
                if (overtime)
                {
                    Close();
                }
                waitTimeout?.Cancel();
            }
            catch (System.Exception)
            {
                waitTimeout?.Advance();
                Close();
                throw;
            }
            return !overtime;
        }
    }

    public class SocketPipeClient : SocketPipe
    {
        public SocketPipeClient(string pipeName, string serverName = ".")
        {
            _socket = new NamedPipeClientStream(serverName, "Yu3zxGo_" + pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public NamedPipeClientStream GoSocket
        {
            get
            {
                return (NamedPipeClientStream)_socket;
            }
        }

        public override void Close()
        {
            if (_socket.IsConnected)
            {
                try
                {
                    cancel_io(_socket.SafePipeHandle);
                }
                catch (System.Exception) { }
            }
            try
            {
                _socket.Close();
            }
            catch (System.Exception) { }
        }

        public bool try_connect()
        {
            try
            {
                ((NamedPipeClientStream)_socket).Connect(0);
                return true;
            }
            catch (System.Exception) { }
            return false;
        }

        public async Task<bool> connect(int ms = -1)
        {
            if (0 == ms)
            {
                return try_connect();
            }
            long beginTick = SystemTick.GetTickUs();
            while (!try_connect())
            {
                await Generator.sleep(1);
                if (ms >= 0 && SystemTick.GetTickUs() - beginTick >= (long)ms * 1000)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
