namespace Yu3zx.Devices
{
    interface IConnector
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        bool Init();
        /// <summary>
        /// 打开链接
        /// </summary>
        /// <returns></returns>
        bool Open();
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        bool Close();
        /// <summary>
        /// 是否已链接
        /// </summary>
        bool Connected { get; }
    }
}
