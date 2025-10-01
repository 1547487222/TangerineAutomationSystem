

using QStandaedPlatform.Engine.Common.Common;

namespace TangerineSerivce
{
    public class AppSerivce : BackgroundService
    {
        private readonly IWorkFlowEngine tangerineTheatreSystem;

        public const string systemId = "AppSystem";
        public const string HomeId = "9B32F4A1C7D84E6B8F1A2B3C4D5E6F7A";
        public const string PreperExperimentId = " 8C2B1A3D4E5F6G7H8I9J0K1L2M3N4O5P";
        public const string StartTaskId = " FEDCBA9876543210ABCDEF1234567890";
        public const string FinalizeId = "1234567890ABCDEF1234567890ABCDEF";
        public const string MaintenanceId = "AABBCCDDEEFF11223344556677889900";
        public const string SystemStorageId = "11223344556677889900AABBCCDDEEFF";



        public AppSerivce()
        {
            tangerineTheatreSystem = WorkFlowEngine.Instance;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeAsync();
        }


        public IWorkFlowEngine TheatreSystem => tangerineTheatreSystem;

        /// <summary>
        /// 初始化
        /// </summary>
        public Flow? HomeCineDesign { get; private set; }
        /// <summary>
        /// 实验前准备
        /// </summary>
        public Flow? PreperExperimentCineDesign { get; private set; }
        /// <summary>
        /// 实验任务
        /// </summary>
        public Flow? StartTaskCineDesign { get; private set; }
        /// <summary>
        /// 实验后处理
        /// </summary>
        public Flow? FinalizeCineDesign { get; private set; }
        /// <summary>
        /// 仪器维护
        /// </summary>
        public Flow? MaintenanceCineDesign { get; private set; }

        /// <summary>
        /// 系统封存
        /// </summary>
        public Flow? SystemStorageCineDesign { get; private set; }

        public Task InitializeAsync()
        {
            //tangerineTheatreSystem.RenderCapture();
            //tangerineTheatreSystem.AllocateFundStageCinemaDesign();
            //HomeCineDesign = tangerineTheatreSystem[HomeId];
            //PreperExperimentCineDesign = tangerineTheatreSystem[PreperExperimentId];
            //StartTaskCineDesign = tangerineTheatreSystem[StartTaskId];
            //FinalizeCineDesign = tangerineTheatreSystem[FinalizeId];
            //MaintenanceCineDesign = tangerineTheatreSystem[MaintenanceId];
            //SystemStorageCineDesign = tangerineTheatreSystem[SystemStorageId];
            return Task.CompletedTask;
        }
    }
}