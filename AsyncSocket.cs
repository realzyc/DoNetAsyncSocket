/********************************************************* {COPYRIGHT-TOP} ***
 * RealZYC Confidential
 * OCO Source Materials
 *
 * (C) Copyright RealZYC Corp. 2013 All Rights Reserved.
 *
 * The source code for this program is not published or otherwise
 * divested of its trade secrets, irrespective of what has been
 * deposited with the China Copyright Office.
 ********************************************************** {COPYRIGHT-END} **/
using System;
using System.Net.Sockets;

namespace AsyncSocketLib
{
    /// <summary>
    /// AsyncSocket provides easy-to-use and powerful asynchronous socket libraries for .Net framework. 
    /// </summary>
    public class AsyncSocket: IDisposable
    {
        /***************************************
         * Enum
         ***************************************/
        #region Enum
        /// <summary>
        /// The state of socket
        /// </summary>
        public enum SocketState : int
        {
            /// <summary> Connection closed </summary>
            Closed = 0,
            /// <summary> Bound to local end point </summary>
            Bound = 1,
            /// <summary> Listening for client </summary>
            Listening = 2,
            /// <summary> Connecting to listener </summary>
            Connecting = 3,
            /// <summary> Connected </summary>
            Connected = 4
        }
        /// <summary>
        /// The type of socket error
        /// </summary>
        public enum SocketError : int
        {
            /// <summary> Unknown Error </summary>
            Unknown = -1,
            /// <summary> No Error </summary>
            NoError = 0,
            /// <summary> Local close the connection </summary>
            LocalClose = 1,
            /// <summary> Remote close the connection </summary>
            RemoteClose = 2,
            /// <summary> Unavailable end point is used </summary>
            EndPointUnavailable = 3,
            /// <summary> The base socket is unavailable </summary>
            SocketUnavailable = 4,
            /// <summary> The base socket is unavailable </summary>
            MemoryInaccessible = 5,
            /// <summary> A invalid operation occurs </summary>
            InvalidOperation = 6,
            /// <summary> The buffer is unavailable </summary>
            BufferUnavailable = 7,
            /// <summary> The operation is time out </summary>
            OperationTimeOut = 8,
            /// <summary> The port in already in used </summary>
            PortInUsed = 9
        }
        /// <summary>
        /// The type of how to raise socket state changed event
        /// </summary>
        protected enum SocketStateRaiseEventType : int
        {
            /// <summary> Raise event if state changed </summary>
            IfChanged = 0,
            /// <summary> Always raise event </summary>
            Always = 1,
            /// <summary> Never raise event </summary>
            Never = 2,
        }

        #endregion

        /***************************************
         * SubClass - SocketBuffer
         ***************************************/
        #region SubClass - SocketBuffer
        /// <summary>
        /// The socket buffer
        /// </summary>
        public class SocketBuffer : IDisposable
        {
            /// <summary> The bytes in buffer, form 0 to AvailableLength - 1 are available </summary>
            protected byte[] bytes_Bytes = new byte[] { 0 };
            /// <summary> The available count of byte </summary>
            protected int int_AvailableLength = 0;
            /// <summary> The bytes in buffer, form 0 to AvailableLength - 1 are available </summary>
            public Byte[] Bytes
            {
                get { return bytes_Bytes; }
            }
            /// <summary> The available count of byte </summary>
            public int AvailableLength
            {
                get { return int_AvailableLength; }
            }
            /// <summary> The buffer size, the previous data will lost if changed. </summary>
            public int Size
            {
                get { return bytes_Bytes.Length; }
                set
                {
                    if (value < 1) value = 1;
                    if (value != bytes_Bytes.Length)
                    {
                        bytes_Bytes = null;
                        bytes_Bytes = new byte[value];
                    }
                }
            }
            /// <summary>
            /// Initial set
            /// </summary>
            /// <param name="Bytes">The bytes in buffer, form 0 to AvailableLength - 1 are available</param>
            /// <param name="AvailableLength">The available count of byte</param>
            public SocketBuffer(Byte[] Bytes, int AvailableLength)
            {
                bytes_Bytes = Bytes;
                int_AvailableLength = AvailableLength;
            }
            /// <summary>
            /// Initial set
            /// </summary>
            /// <param name="Size">The buffer size</param>
            public SocketBuffer(int Size)
            {
                this.Size = Size;
                int_AvailableLength = 0;
            }
            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                bytes_Bytes = null;
            }
        }

        #endregion

        /***************************************
         * SubClass - RequestEventArgs
         ***************************************/
        #region SubClass - RequestEventArgs
        /// <summary>
        /// The event args of connect request
        /// </summary>
        public class RequestEventArgs : EventArgs
        {
            /// <summary> Initial set </summary>
            /// <param name="AsyncResult">The async result given by request event</param>
            public RequestEventArgs(IAsyncResult AsyncResult)
            {
                async_AsyncResult = AsyncResult;
            }
            /// <summary> The async result given by request event </summary>
            protected IAsyncResult async_AsyncResult = null;
            /// <summary> The async result given by request event </summary>
            public IAsyncResult AsyncResult
            {
                get { return async_AsyncResult; }
            }
        }

        #endregion

        /***************************************
         * SubClass - DataReceivedEventArgs
         ***************************************/
        #region SubClass - DataReceivedEventArgs
        /// <summary>
        /// The event args of data received
        /// </summary>
        public class DataReceivedEventArgs : EventArgs
        {
            /// <summary> The current received buffer of socket </summary>
            protected SocketBuffer buffer_CurrentReceiveBuffer = null;
            /// <summary> The current received buffer of socket </summary>
            public SocketBuffer CurrentReceiveBuffer
            {
                get { return buffer_CurrentReceiveBuffer; }
            }
            /// <summary>
            /// Initial set
            /// </summary>
            /// <param name="CurrentReceiveBuffer">The current received buffer of socket</param>
            public DataReceivedEventArgs(SocketBuffer CurrentReceiveBuffer)
            {
                buffer_CurrentReceiveBuffer = CurrentReceiveBuffer;
            }
        }

        #endregion

        /***************************************
         * SubClass - DataSentEventArgs
         ***************************************/
        #region SubClass - DataSentEventArgs
        /// <summary>
        /// The event args of data sent
        /// </summary>
        public class DataSentEventArgs : EventArgs
        {
            /// <summary> The count of sent byte </summary>
            protected int int_ByteSent = 0;
            /// <summary> The count of sent byte </summary>
            public int ByteSent
            {
                get { return int_ByteSent; }
            }
            /// <summary>
            /// Initial set
            /// </summary>
            /// <param name="ByteSent">The count of sent byte</param>
            public DataSentEventArgs(int ByteSent)
            {
                if (ByteSent < 0) ByteSent = 0;
                int_ByteSent = ByteSent;
            }
        }

        #endregion

        /***************************************
         * SubClass - StateChangedEventArgs
         ***************************************/
        #region SubClass - StateChangedEventArgs
        /// <summary>
        /// The event args of socket state changed
        /// </summary>
        public class StateChangedEventArgs : EventArgs
        {
            /// <summary> The current socket state </summary>
            protected SocketState enum_CurrentState = SocketState.Closed;
            /// <summary> The last socket state </summary>
            protected SocketState enum_LastState = SocketState.Closed;
            /// <summary> The error type of socket if state is "Errors" </summary>
            protected SocketError enum_ErrorType = SocketError.NoError;
            /// <summary> The current socket state </summary>
            public SocketState CurrentState
            {
                get { return enum_CurrentState; }
            }
            /// <summary> The last socket state </summary>
            public SocketState LastState
            {
                get { return enum_LastState; }
            }
            /// <summary> The error type of socket if state is "Errors" </summary>
            public SocketError ErrorType
            {
                get { return enum_ErrorType; }
            }
            /// <summary>
            /// Initial set
            /// </summary>
            /// <param name="LastState">The last socket state</param>
            /// <param name="CurrentState">The current socket state</param>
            /// <param name="ErrorType">The error type of socket if state is "Errors"</param>
            public StateChangedEventArgs(SocketState LastState,
                SocketState CurrentState,
                SocketError ErrorType)
            {
                this.enum_LastState = LastState;
                this.enum_CurrentState = CurrentState;
                this.enum_ErrorType = ErrorType;
            }
        }

        #endregion

        /***************************************
         * Event
         ***************************************/
        #region Event
        /// <summary>
        /// The event of socket state changed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event args</param>
        public delegate void StateChangedDelegate(object sender, StateChangedEventArgs e);
        /// <summary>
        /// The event of socket state changed
        /// </summary>
        public event StateChangedDelegate StateChanged;
        /// <summary>
        /// Raise state changed event
        /// </summary>
        protected void OnStateChanged(StateChangedEventArgs e)
        {
            if (StateChanged != null) StateChanged(this, e);
        }

        /// <summary>
        /// The event of connect request
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event args</param>
        public delegate void RequestDelegate(object sender, RequestEventArgs e);
        /// <summary>
        /// The event of connect request
        /// </summary>
        public event RequestDelegate Request;
        /// <summary>
        /// Raise connect request event
        /// </summary>
        protected void OnRequest(RequestEventArgs e)
        {
            if (Request != null) Request(this, e);
        }

        /// <summary>
        /// The event of data received
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event args</param>
        public delegate void DataReceivedDelegate(object sender, DataReceivedEventArgs e);
        /// <summary>
        /// The event of data received
        /// </summary>
        public event DataReceivedDelegate DataReceived;
        /// <summary>
        /// Raise data received event
        /// </summary>
        protected void OnDataReceived(DataReceivedEventArgs e)
        {
            if (DataReceived != null) DataReceived(this, e);
        }

        /// <summary>
        /// The event of data sent
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event args</param>
        public delegate void DataSentDelegate(object sender, DataSentEventArgs e);
        /// <summary>
        /// The event of data sent
        /// </summary>
        public event DataSentDelegate DataSent;
        /// <summary>
        /// Raise data sent event
        /// </summary>
        protected void OnDataSent(DataSentEventArgs e)
        {
            if (DataSent != null) DataSent(this, e);
        }

        #endregion

        /***************************************
         * Value
         ***************************************/
        #region Value
        /// <summary>
        /// The core base socket
        /// </summary>
        protected Socket socket_Base = null;

        /// <summary>
        /// The received buffer
        /// </summary>
        protected SocketBuffer buffer_ReceiveBuffer = new SocketBuffer(8192);   //Socket的默认接收缓冲的大小为8192

        /// <summary>
        /// The socket flags for receive
        /// </summary>
        protected SocketFlags enum_ReceiveFlags = SocketFlags.None;

        /// <summary>
        /// The state of socket
        /// </summary>
        protected SocketState enum_State = SocketState.Closed;

        /// <summary>
        /// The state show that the socket is on accepting to connection
        /// </summary>
        protected bool bool_IsOnAccepting = false;

        /// <summary>
        /// The state show that the socket is on sending data
        /// </summary>
        protected bool bool_IsOnSendingData = false;

        /// <summary>
        /// The state show that the socket is on receiving data
        /// </summary>
        protected bool bool_IsOnReceivingData = false;

        #endregion

        /***************************************
         * Property
         ***************************************/
        #region Property
        /// <summary>
        /// The base socket for network connection
        /// </summary>
        public Socket Base
        {
            get { return socket_Base; }
            set
            {
                if (socket_Base != value && socket_Base != null)
                {
                    Close();
                }
                socket_Base = value;
                if (socket_Base != null)
                {
                    if (socket_Base.Connected)
                    {
                        this.enum_State = SocketState.Connected;
                        this.bool_IsOnReceivingData = false;
                        timer_TimerReceive.Start();
                    }
                    else
                    {
                        this.enum_State = SocketState.Closed;
                    }
                }
            }
        }

        /// <summary>
        /// The received buffer, which remembered the last received data. One can also change buffer size by it
        /// </summary>
        public SocketBuffer ReceiveBuffer
        {
            get { return buffer_ReceiveBuffer; }
        }

        /// <summary>
        /// The state of socket
        /// </summary>
        public SocketState State
        {
            get { return enum_State; }
        }

        /// <summary>
        /// The socket flags for receive
        /// </summary>
        public SocketFlags ReceiveFlags
        {
            get { return enum_ReceiveFlags; }
            set { enum_ReceiveFlags = value; }
        }

        /// <summary>
        /// The state show that the socket is on sending data
        /// </summary>
        public bool IsOnSendingData
        {
            get { return bool_IsOnSendingData; }
        }

        /// <summary>
        /// The state show that the socket is on receiving data
        /// </summary>
        public bool IsOnReceivingData
        {
            get { return bool_IsOnReceivingData; }
        }

        /// <summary>
        /// The state show that the socket is on accepting to connection
        /// </summary>
        public bool IsOnAccepting
        {
            get { return bool_IsOnAccepting; }
        }

        /// <summary>
        /// Set/Get the interval of trying listening
        /// </summary>
        public double ListenInterval
        {
            get { return timer_TimerListen.Interval; }
            set
            {
                if (value < 1) value = 1;
                timer_TimerListen.Interval = value;
            }
        }

        /// <summary>
        /// Set/Get the interval of trying receiving data
        /// </summary>
        public double ReceiveInterval
        {
            get { return timer_TimerReceive.Interval; }
            set
            {
                if (value < 1) value = 1;
                timer_TimerReceive.Interval = value;
            }
        }

        #endregion

        /***************************************
         * New Sub
         ***************************************/
        #region New Sub
        /// <summary>
        /// Initial set sub
        /// </summary>
        /// <param name="Base">The base socket</param>
        /// <remarks>Example: BaseSocket:= New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        /// Or BaseSocket:= New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)</remarks>
        protected void InitialSet(Socket Base)
        {
            this.Base = Base;

            timer_TimerListen.Elapsed +=new System.Timers.ElapsedEventHandler(TimerListen_Elapsed);
            timer_TimerReceive.Elapsed +=new System.Timers.ElapsedEventHandler(TimerReceive_Elapsed);
        }

        /// <summary>
        /// Initial set sub
        /// </summary>
        /// <param name="AddressFamily">The address family of socket</param>
        /// <param name="SocketType">The socket type of socket</param>
        /// <param name="ProtocolType">The protocol type of socket</param>
        protected void InitialSet(System.Net.Sockets.AddressFamily AddressFamily,
            System.Net.Sockets.SocketType SocketType,
            System.Net.Sockets.ProtocolType ProtocolType)
        {
            InitialSet(new Socket(AddressFamily, SocketType, ProtocolType));
        }

        /// <summary>
        /// Initial set sub
        /// </summary>
        /// <param name="Base">The base socket</param>
        /// <remarks>Example: BaseSocket:= New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        /// Or BaseSocket:= New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)</remarks>
        public AsyncSocket(Socket Base)
        {
            InitialSet(Base);
        }

        /// <summary>
        /// Initial set sub
        /// </summary>
        /// <param name="AddressFamily">The address family of socket</param>
        /// <param name="SocketType">The socket type of socket</param>
        /// <param name="ProtocolType">The protocol type of socket</param>
        public AsyncSocket(System.Net.Sockets.AddressFamily AddressFamily,
            System.Net.Sockets.SocketType SocketType,
            System.Net.Sockets.ProtocolType ProtocolType)
        {
            InitialSet(AddressFamily, SocketType, ProtocolType);
        }

        #endregion

        /***************************************
         * Public Function
         ***************************************/
        #region Public Function
        /// <summary>
        /// Close connection or stop connecting / listening
        /// </summary>
        /// <returns>Whether succeed in any operaion</returns>
        public bool Close()
        {
            try
            {
                ChangeSocketState(SocketState.Closed, SocketError.LocalClose, SocketStateRaiseEventType.IfChanged);
            }
            catch { }
            return true;
        }

        /// <summary>
        /// Bind the local end point to socket before start listen
        /// </summary>
        /// <param name="LocalEP">The local end point to bind</param>
        /// <returns>Whether succeed</returns>
        public bool Bind(System.Net.EndPoint LocalEP)
        {
            if (this.State != SocketState.Closed) this.Close();
            try
            {
                ChangeSocketState(SocketState.Bound, SocketError.NoError, SocketStateRaiseEventType.IfChanged);
                socket_Base.Bind(LocalEP);
                return true;
            }
            catch (System.ArgumentNullException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.EndPointUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.Net.Sockets.SocketException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.EndPointUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.ObjectDisposedException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.Security.SecurityException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.MemoryInaccessible, SocketStateRaiseEventType.IfChanged);
            }
            catch
            {
                ChangeSocketState(SocketState.Closed, SocketError.Unknown, SocketStateRaiseEventType.IfChanged);
            }
            return false;
        }

        /// <summary>
        /// Listen to any connection, bind function has been used before.
        /// </summary>
        /// <returns>Whether succeed</returns>
        /// <remarks>Equal to Listen(65535)</remarks>
        public bool Listen()
        {
            return Listen(65535);
        }
        /*Public Function Listen() As Boolean
            Return Listen(65535)
        End Function*/

        /// <summary>
        /// Listen to any connection, bind function has been used before.
        /// </summary>
        /// <param name="Backlog">The max count of waiting connection</param>
        /// <returns>Whether succeed</returns>
        public bool Listen(int Backlog)
        {
            if (this.State != SocketState.Closed && this.State != SocketState.Bound) this.Close();
            try
            {
                //使用异步侦听
                ChangeSocketState(SocketState.Listening, SocketError.NoError, SocketStateRaiseEventType.IfChanged);

                socket_Base.Listen(Backlog);  //Listen参数

                bool_IsOnAccepting = false; //重置状态变量
                timer_TimerListen.Start();

                return true;
            }
            catch(System.Net.Sockets.SocketException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.EndPointUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch(System.ObjectDisposedException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch
            {
                ChangeSocketState(SocketState.Closed, SocketError.Unknown, SocketStateRaiseEventType.IfChanged);
            }
            return false;
        }

        /// <summary>
        /// Accept a requested connection and get a new linked netsocket
        /// </summary>
        /// <param name="AsyncResult">The async result given by event args in request event</param>
        /// <returns>The new linked netsocket</returns>
        /// <remarks>User the function only in request event, return nothing with any error</remarks>
        public AsyncSocket Accept(IAsyncResult AsyncResult)
        {
            AsyncSocket NewSocket = null;
            try
            {
                Socket BaseSocket = socket_Base.EndAccept(AsyncResult);
                if (BaseSocket != null)
                {
                    NewSocket = new AsyncSocket(BaseSocket);
                }
            }
            catch
            {
                NewSocket = null;
            }
            return NewSocket;
        }

        /// <summary>
        /// Connect to the remote end point
        /// </summary>
        /// <param name="RemoteEP">The remote end point</param>
        /// <returns>Whether succeed</returns>
        public bool Connect(System.Net.EndPoint RemoteEP)
        {
            if (this.State != SocketState.Closed && this.State != SocketState.Bound) this.Close();
            //使用异步连接
            try
            {
                ChangeSocketState(SocketState.Connecting, SocketError.NoError, SocketStateRaiseEventType.IfChanged);
                socket_Base.BeginConnect(RemoteEP, new AsyncCallback(AsyncConnectEnd), socket_Base);
                return true;
            }
            catch (System.ArgumentNullException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.EndPointUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.Net.Sockets.SocketException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.ObjectDisposedException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.Security.SecurityException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.MemoryInaccessible, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.InvalidOperationException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.InvalidOperation, SocketStateRaiseEventType.IfChanged);
            }
            catch
            {
                ChangeSocketState(SocketState.Closed, SocketError.Unknown, SocketStateRaiseEventType.IfChanged);
            }
            return false;
        }

        /// <summary>
        /// Send data to remote point, when the socket is connected.
        /// </summary>
        /// <param name="SendBytes">The bytes to send</param>
        /// <param name="Offset">The start index in byte</param>
        /// <param name="Size">The length of the byte to send</param>
        /// <param name="SocketFlags">The socket flags</param>
        /// <returns>Whether successed</returns>
        public bool SendData(Byte[] SendBytes, int Offset, int Size, SocketFlags SocketFlags)
        {
            this.bool_IsOnSendingData = true;
            try
            {
                if (SendBytes != null && Size > 0)
                {
                    socket_Base.BeginSend(SendBytes, Offset, Size, SocketFlags, 
                                                                  new AsyncCallback(AsyncSendEnd), socket_Base);
                    return true;
                }
            }
            catch (System.ArgumentNullException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.BufferUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.Net.Sockets.SocketException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.BufferUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch (ObjectDisposedException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch
            {
                ChangeSocketState(SocketState.Closed, SocketError.Unknown, SocketStateRaiseEventType.IfChanged);
            }
            return false;
        }

        #endregion

        /***************************************
         * Protected Function
         ***************************************/
        #region Protected Function
         /// <summary>
        /// Change socket state and raise socket state changed event
        /// </summary>
        /// <param name="NewState">The new socket state</param>
        /// <param name="ErrorType">The error type of socket if state is "Errors"</param>
        /// <param name="RaiseEventType">The raise event type</param>
        protected void ChangeSocketState(SocketState NewState, 
            SocketError ErrorType, SocketStateRaiseEventType RaiseEventType)
        {
            SocketState LastState = this.State;
            this.enum_State = NewState;   //类内唯一设置状态的语句

            //要在Me.enum_State = NewState之后
            if (NewState == SocketState.Closed)
            {
                ResetSocket();
            }

            if (RaiseEventType == SocketStateRaiseEventType.Always || 
                (RaiseEventType == SocketStateRaiseEventType.IfChanged && NewState != LastState))
            {
                OnStateChanged(new StateChangedEventArgs(LastState, NewState, ErrorType));
            }
        }

        /// <summary>
        /// Reset socket to ready-to-connect state
        /// </summary>
        protected void ResetSocket()
        {
            //不报错
            //先将状态设置为Closed, 这样就能保证由Shutdown引发的一系列事件不反应到外部
            try
            {
                if (socket_Base != null)
                {
                    socket_Base.Shutdown(SocketShutdown.Both);
                    socket_Base.Disconnect(false); //关闭套接字并试图重用失败. 不能使用Bind
                }
            }
            catch { }

            //停止接收线程
            timer_TimerReceive.Stop();
            //停止监听线程
            timer_TimerListen.Stop();

            Socket NewSocket = new Socket(socket_Base.AddressFamily, socket_Base.SocketType, socket_Base.ProtocolType);
            socket_Base.Close();                   //释放所有资源.
            socket_Base = NewSocket;
        }

        #endregion

        /***************************************
         * Timer function
         ***************************************/
        #region Timer function
        /// <summary> The timer of listening to connection </summary>
        protected System.Timers.Timer timer_TimerListen = new System.Timers.Timer();
        /// <summary> The timer of receiving data  </summary>
        protected System.Timers.Timer timer_TimerReceive = new System.Timers.Timer();

        /// <summary>
        /// The timer function of listening to connection
        /// </summary>
        protected void TimerListen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer_TimerListen.Stop();
            try
            {
                if(!bool_IsOnAccepting)
                {
                    bool_IsOnAccepting = true;
                    socket_Base.BeginAccept(new AsyncCallback(AsyncAcceptEnd), socket_Base);
                }
            }
            catch(System.ObjectDisposedException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch(System.NotSupportedException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.InvalidOperation, SocketStateRaiseEventType.IfChanged);
            }
            catch(System.InvalidOperationException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.InvalidOperation, SocketStateRaiseEventType.IfChanged);
            }
            catch(System.ArgumentOutOfRangeException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.BufferUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch(System.Net.Sockets.SocketException)
            {
                ChangeSocketState(SocketState.Closed, SocketError.SocketUnavailable, SocketStateRaiseEventType.IfChanged);
            }
            catch//(Exception ex)
            {
                ChangeSocketState(SocketState.Closed, SocketError.Unknown, SocketStateRaiseEventType.IfChanged);
            }
        }

        /// <summary>
        /// The timer function of receiving data
        /// </summary>
        protected void TimerReceive_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer_TimerReceive.Stop();
            try
            {
                if (!bool_IsOnReceivingData)
                {
                    bool_IsOnReceivingData = true;
                    if (ReceiveBuffer.AvailableLength > 0) //接收缓冲内有内容
                    {
                        SocketBuffer BackBuffer = ReceiveBuffer;
                        this.buffer_ReceiveBuffer = new SocketBuffer(BackBuffer.Size);
                        BackBuffer.Dispose();
                        BackBuffer = null;
                    }
                    socket_Base.BeginReceive(ReceiveBuffer.Bytes, 0, ReceiveBuffer.Size,
                        enum_ReceiveFlags,
                        new AsyncCallback(AsyncReceiveEnd), socket_Base);
                }
            }
            catch
            {
                //判断类型, 视之为断开, 并引发Closed事件
                //在Closed中已经进行了Socket重置
                ChangeSocketState(SocketState.Closed, SocketError.RemoteClose, SocketStateRaiseEventType.IfChanged);
            }
        }

        #endregion

        /***************************************
         * Async function
         ***************************************/
        #region Async function
        /// <summary>
        /// The call back function of async accept connection
        /// </summary>
        /// <param name="AsyncResult">The async result</param>
        protected void AsyncAcceptEnd(IAsyncResult AsyncResult)
        {
            try
            {
                if (this.State == SocketState.Listening)//注意在其他状态不能监听状态
                {
                    //引发Request事件, 传入得到的新的Me到事件参数e中
                    OnRequest(new RequestEventArgs(AsyncResult));
                    bool_IsOnAccepting = false; //控制变量=False, 允许主进程继续使用BeginAccept
                    timer_TimerListen.Start();  //再次启动
                }
                else
                {
                    AsyncSocket NewSocket = Accept(AsyncResult);
                    if (NewSocket != null)
                    {
                        NewSocket.Close();
                        NewSocket.Dispose();
                    }
                    NewSocket = null;
                    bool_IsOnAccepting = false;
                }
            }
            catch { }
        }

        /// <summary>
        /// The call back function of async connect
        /// </summary>
        /// <param name="AsyncResult">The async result</param>
        protected void AsyncConnectEnd(IAsyncResult AsyncResult)
        {
            try
            {
                socket_Base.EndConnect(AsyncResult);
                ChangeSocketState(SocketState.Connected, SocketError.NoError, SocketStateRaiseEventType.IfChanged);

                this.bool_IsOnReceivingData = false;
                timer_TimerReceive.Start();
            }
            catch //(Exception ex)
            {
                ChangeSocketState(SocketState.Closed, SocketError.OperationTimeOut, SocketStateRaiseEventType.IfChanged);
            }
        }

        /// <summary>
        /// The call back function of async send
        /// </summary>
        /// <param name="AsyncResult">The async result</param>
        protected void AsyncSendEnd(IAsyncResult AsyncResult)
        {
            int ByteSent = 0;
            try
            {
                ByteSent = socket_Base.EndSend(AsyncResult);
            }
            catch
            {
                ByteSent = 0;
            }
            if (this.State == SocketState.Connected) //只有在状态正确时才引发事件
            {
                OnDataSent(new DataSentEventArgs(ByteSent));
            }
            this.bool_IsOnSendingData = false;
        }

        /// <summary>
        /// The call back function of async receive
        /// </summary>
        /// <param name="AsyncResult">The async result</param>
        protected void AsyncReceiveEnd(IAsyncResult AsyncResult)
        {
            int ByteReceived = 0;
            try
            {
                ByteReceived = socket_Base.EndReceive(AsyncResult);
            }
            catch (ArgumentException)
            { //此错误出现在当上一个链接已经关闭, 建立了新的socket, 而新一轮的Receive已经开始
                ByteReceived = -1;
            }
            catch
            {
                ByteReceived = 0;
            }

            if (ByteReceived > 0)
            {
                if (State == SocketState.Connected) //判断状态
                {
                    byte[] ReceivedByte = this.buffer_ReceiveBuffer.Bytes; //获取获得的字节
                    this.buffer_ReceiveBuffer.Dispose();                                //释放上次的缓冲区
                    this.buffer_ReceiveBuffer = null;
                    this.buffer_ReceiveBuffer = new SocketBuffer(ReceivedByte, ByteReceived); //建立新缓冲区, 传入字节
                    OnDataReceived(new DataReceivedEventArgs(this.buffer_ReceiveBuffer));
                }
                bool_IsOnReceivingData = false; //控制变量=False, 允许主进程继续使用BeginReceivingData
                if (this.State == SocketState.Connected)
                {
                    timer_TimerReceive.Start();  //再次启动
                }
            }
            else if (ByteReceived == -1)
            {//忽略这种错误, 保持链接
                bool_IsOnReceivingData = false;
            }
            else
            {
                bool_IsOnReceivingData = false;  //控制变量=False, 允许主进程继续使用BeginReceivingData
                //如果没接收到数据, 认为是端口关闭
                ChangeSocketState(SocketState.Closed, SocketError.RemoteClose, SocketStateRaiseEventType.IfChanged);
            }
        }

        #endregion

        /***************************************
         * IDisposable
         ***************************************/
        #region IDisposable
        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            Close();
            if (socket_Base != null)
            {
                socket_Base = null;
            }
            if (buffer_ReceiveBuffer != null)
            {
                buffer_ReceiveBuffer.Dispose();
                buffer_ReceiveBuffer = null;
            }
        }

        #endregion

    }//class AsyncSocket
}//namespace
