using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using SqlSugar;
using System.Dynamic;
using System.Net.Http.Headers;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    #region 托盘区域
    public class TrayRegion : ICloneable
    {
        private readonly List<QWell> _regionWells = [];
        /// <summary>
        /// 托盘区域ID
        /// </summary>
        public long TrayRegionId { get; set; }
        /// <summary>
        /// 托盘区域名称
        /// </summary>
        public string TrayRegionName { get; set; } = string.Empty;
        /// <summary>
        /// 托盘区域类型
        /// </summary>
        public LabTrayDefaultType TrayRegionType { get; set; }
        /// <summary>
        ///托盘区域总行数
        /// </summary>
        public int Rows { get; set; }
        /// <summary>
        /// 托盘区域总列数
        /// </summary>
        public int Cols { get; set; }
        /// <summary>
        /// 托盘区域行间距
        /// </summary>
        public float SpaceRow { get; set; }
        /// <summary>
        /// 托盘区域列间距
        /// </summary>
        public float SpaceCol { get; set; }
        /// <summary>
        /// 托盘区域行标签
        /// </summary>
        public List<string> RowLabels { get; set; } = [];
        /// <summary>
        /// 托盘区域列标签
        /// </summary>
        public List<string> ColLabels { get; set; } = [];
        /// <summary>
        /// 托盘区域起始位置
        /// </summary>
        public QPosition StartPosition { get; set; } = new();
        /// <summary>
        /// 夹爪设置
        /// </summary>
        public ClawGraspInfo ClawSetting { get; set; } = new();
        /// <summary>
        /// 是否物理虚拟区域
        /// </summary>
        public bool VirtualRegion { get; set; } = false;

        public void InitRegionWells(List<QWell> wells)
        {
            _regionWells.Clear();
            _regionWells.AddRange(wells);
        }
        public List<QWell> GetRegionWells()
        {
            return _regionWells;
        }

        /// <summary>
        /// 通过材料id查找指定孔位
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell GetWellByMaterialId(long materialId)
        {
            var well = _regionWells
                .FirstOrDefault(w => 
                w.Labware?.Material?.MaterialNo == materialId) ?? throw new Exception($"未找到指定材料id的孔位");
            if (well.Status != WellStatus.Loaded
                || well.Status != WellStatus.Returned)
            {
                throw new Exception($"找到对应孔位{well},但孔位状态不符。");
            }
            return well;
        }
        public object Clone()
        {
            var clone = new TrayRegion
            {
                Rows = Rows,
                Cols = Cols,
                SpaceRow = SpaceRow,
                SpaceCol = SpaceCol,
                TrayRegionId = TrayRegionId,
                TrayRegionName = TrayRegionName,
                VirtualRegion = VirtualRegion,
                RowLabels = [.. RowLabels],
                ColLabels = [.. ColLabels],
                TrayRegionType = TrayRegionType,
                StartPosition = (QPosition)StartPosition.Clone(),
                ClawSetting = (ClawGraspInfo)ClawSetting.Clone()
            };
            return clone;
        }
    }
    #endregion
    public class LabTray :  ISetupLabTray, IParameter, ICloneable
    {
        private readonly Dictionary<string, QWell> _wells = [];
        private readonly ILogger<LabTray> _logger;
        private volatile int _wellIndex = 0;
        public LabTray()
        {
            LabTrayCategory = string.Empty;
            LabTrayName = string.Empty;
            LabTrayCode = string.Empty;
            LabTrayDescription = string.Empty;
            LabwareName = string.Empty;
            Regions = [];
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<LabTray>();
        }
        public string LabTrayCategory { get; set; }
        public string LabTrayName { get; set; }
        public string LabTrayCode { get; set; }
        public string LabTrayDescription { get; set; }
        public string LabwareName { get; set; }
        public Guid ParameterId { get; set; }
        public long LabTrayId { get; set; }
        public bool RowFirst { get; set; } = true;

        public bool VirtualTray { get; set; } = false;

        public List<TrayRegion> Regions { get; set; }

        /// <summary>
        /// 孔位绑定信息
        /// </summary>
        public TrayInitialBindingInfo InitialBindingInfo { get; set; } = new();


        public void InitlizeParameter()
        {
            InitLabTray();
            Log("初始化LabTray完成");
        }

        public void InitLabTray()
        {
			Interlocked.Exchange(ref _wellIndex, 0);

            if (_wells.Count > 0)
            {
                _wells.Clear();
            }
            foreach (var region in Regions)
            {
                var regionWells = new List<QWell>();
                if (region.RowLabels.Count != region.Rows || region.ColLabels.Count != region.Cols)
                {
                    throw new Exception($"区域标签数量与行列数不匹配");
                }

                if (RowFirst)
                {
                    for (int row = 0; row < region.Rows; row++)
                    {
                        for (int col = 0; col < region.Cols; col++)
                        {
                            float posX = region.StartPosition.X + col * region.SpaceCol;
                            float posY = region.StartPosition.Y + row * region.SpaceRow;

                            string label = $"{region.RowLabels[row]}{region.ColLabels[col]}";

                            var well = new QWell
                            {
                                Label = label,
                                LabwareName = LabwareName,
                                LabTrayName = LabTrayName,
                                LabTrayId = LabTrayId,
                                Position = new QPosition
                                {
                                    X = posX,
                                    Y = posY,
                                    Z = region.StartPosition.Z,
                                    Z2 = region.StartPosition.Z2,
                                    Angle = region.StartPosition.Angle,
                                    Depth = region.StartPosition.Depth,
                                    ZPutGetOffset = region.StartPosition.ZPutGetOffset,
                                },
                                ClawSetting = new ClawGraspInfo
                                {
                                     Angle = region.ClawSetting.Angle,
                                     OpenPos = region.ClawSetting.OpenPos,
                                }
                            };

                            _wells[label] = well;
                            regionWells.Add(well);
                        }
                    }
                }
                else // 列优先
                {
                    for (int col = 0; col < region.Cols; col++)
                    {
                        for (int row = 0; row < region.Rows; row++)
                        {
                            float posX = region.StartPosition.X + col * region.SpaceCol;
                            float posY = region.StartPosition.Y + row * region.SpaceRow;

                            string label = $"{region.RowLabels[row]}{region.ColLabels[col]}";

                            var well = new QWell
                            {
                                Label = label,
                                LabwareName = LabwareName,
                                LabTrayName = LabTrayName,
                                LabTrayId = LabTrayId,
                                Position = new QPosition
                                {
                                    X = posX,
                                    Y = posY,
                                    Z = region.StartPosition.Z,
                                    Z2 = region.StartPosition.Z2,
                                    Angle = region.StartPosition.Angle,
                                    Depth = region.StartPosition.Depth,
                                    ZPutGetOffset = region.StartPosition.ZPutGetOffset,
                                },
                                ClawSetting = new ClawGraspInfo
                                {
                                    Angle = region.ClawSetting.Angle,
                                    OpenPos = region.ClawSetting.OpenPos,
                                }
                            };

                            _wells[label] = well;
                            regionWells.Add(well);
                        }
                    }
                }
               region.InitRegionWells(regionWells);
            }

            InitialBindingInfo.LabTrayId= LabTrayId;
            InitialBindingInfo.LabTrayName = LabTrayName;
            InitialBindingInfo.LabTrayCode = LabTrayCode;
            foreach (var qWell in _wells.Values)
            {
                InitialBindingInfo.WellBindings.Add(new WellInitialBindingInfo
                {
                    WellName = qWell.Label
                });
            }
        }


        public void InitLabTrayInfo(LabTrayInfo labTrayInfo)
        {
            ArgumentNullException.ThrowIfNull(labTrayInfo);
            if (labTrayInfo.LabTrayId != LabTrayId)
                throw new ArgumentException($"LabTrayId 不匹配");
           Log($"初始化托盘 {LabTrayId}: {labTrayInfo}");
            if (_wells.Count == 0)
            {
                InitlizeParameter();
            }
            InitWellStatus(labTrayInfo.WellInfos);
        }

        public void InitWellStatus(List<WellInfo>? wellInfos)
        {
            if (wellInfos != null && wellInfos.Count > 0)
            {
                foreach (var wellInfo in wellInfos)
                {
                    if (_wells.TryGetValue(wellInfo.WellName, out var well))
                    {
                        if (well != null)
                        {
                            well.SetWell(wellInfo);
                            var wellInitialBindingInfo = InitialBindingInfo.WellBindings.First(p => p.WellName == wellInfo.WellName);
                            wellInitialBindingInfo.MaterialId = wellInfo.MaterialId;
                            Log($"初始化孔位: {well}");
                        }
                        else
                        {
                            Log($"初始化孔位状态失败，孔位 {wellInfo.WellName} 为空");
                        }
                    }
                    else
                    {
                        Log($"孔位 {wellInfo.WellName} 不存在");
                        throw new Exception($"孔位 {wellInfo.WellName} 不存在,请检查托盘初始化数据");
                    }
                }
            }
        }
        /// <summary>
        /// 设置孔位二维码
        /// </summary>
        /// <param name="qWell"></param>
        /// <param name="qrCode"></param>
        public void SetQrCode(QWell qWell, string? qrCode)
        {
            if (qWell != null)
            {
                var wellInitialBindingInfo = InitialBindingInfo.WellBindings.FirstOrDefault(p => p.WellName == qWell.Label);
                if (wellInitialBindingInfo != null)
                    wellInitialBindingInfo.QrCode = qrCode ?? string.Empty;
                qWell.Labware?.SetQrCode(qrCode ?? string.Empty);
            }
        }


        /// <summary>
        /// 获取托盘行数
        /// </summary>
        /// <returns></returns>
        public int GetTotalRows()
        {
            lock (_wells)
            {
                var rowPositions = _wells.Values
                .Select(w => w.Position.Y)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

                return rowPositions.Count;
            }
        }
        /// <summary>
        /// 获取托盘列数
        /// </summary>
        /// <returns></returns>
        public int GetTotalCols()
        {
            lock (_wells)
            {
                var colPositions = _wells.Values
                .Select(w => w.Position.X)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

                return colPositions.Count;
            }
        }

        /// <summary>
        /// 是否最终托盘
        /// </summary>
        public bool IsFinalTray => Regions.Any(p => p.TrayRegionType.HasFlag(LabTrayDefaultType.UnloadTray));

        /// <summary>
        /// 是否样品托盘
        /// </summary>
        public bool IsSampleTray => Regions.Any(p => p.TrayRegionType.HasFlag(LabTrayDefaultType.SampleTray));

        /// <summary>
        /// 是否存在空孔位或取出孔位
        /// </summary>
        public bool IsEmptyOrTakenWell => _wells.Values.Any(w => w.Status == WellStatus.Empty || w.Status == WellStatus.Taken);

        public QWell GetWell(string wellName)
        {
            lock (_wells)
            {
                return _wells[wellName];
            }
        }

        public List<QWell> GetWells()
        {
            lock (_wells)
            {
              return  [.._wells.Values];
            }
        }

        /// <summary>
        /// 获取下一个孔位
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (QWell QWell, Action<QWell> successAction) GetNextWell()
        {
            lock (_wells)
            {
                var wells = _wells.Values.OrderBy(w => w.Label).ToArray();
                if (_wellIndex >= wells.Length - 1)
                {
                    Interlocked.Exchange(ref _wellIndex, 0);
                }
                var well = wells.ElementAtOrDefault(_wellIndex) ?? throw new Exception($"未找到{_wellIndex}孔位");
                return (well, successwell =>
                {
                    if (well != successwell)
                        throw new Exception($"孔位不匹配");
                    Interlocked.Increment(ref _wellIndex);
                }
                );
            }
        }

        /// <summary>
        /// 重置孔位索引
        /// </summary>
        public void ResetWellIndex()
        {
            Interlocked.Exchange(ref _wellIndex, 0);
        }

        /// <summary>
        /// 获取所有包含有效物料的孔位，以物料编号 (MaterialNo) 为键。
        /// </summary>
        /// <returns>物料编号 → 孔位 的字典</returns>
        public Dictionary<long, QWell> GetWellsByMaterialNo()
        {
            lock (_wells)
            {
                return _wells.Values
                .Where(well =>
                    well.Labware is not null
                    && well.Labware.Material is not null
                    && well.Labware.Material.MaterialNo is > 0)
                .ToDictionary(
                    well => well.Labware!.Material!.MaterialNo,
                    well => well
                );
            }
        }

        /// <summary>
        /// 获取指定孔位
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public QWell FindWellByIndex(int index)
        {
            lock (_wells)
            {
                var well = _wells.Values.OrderBy(s => s.Label).ToArray();
                if (index >= well.Length) throw new KeyNotFoundException($"未找到索引为 {index} 的孔位");
                return well[index];
            }
        }

        /// <summary>
        /// 获取指定孔位
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public QWell? FindWellByIndexAllowNull(int index)
        {
            lock (_wells)
            {
                var well = _wells.Values.OrderBy(s => s.Label).ToArray();
                if (index >= well.Length) return null;
                return well[index];
            }
        }
        /// <summary>
        /// 获取指定孔位
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public QWell FindWellByMaterialId(long materialId)
        {
            lock (_wells)
            {
                var well = _wells.Values.FirstOrDefault(well =>
                 well.Labware?.Material?.MaterialNo == materialId &&
                 well.Status is WellStatus.Loaded or WellStatus.Returned);
                return well ?? throw new KeyNotFoundException($"未找到包含材料 {materialId} 的孔位");
            }
        }
        /// <summary>
        /// 获取指定孔位(允许为空)
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public QWell? FindWellByMaterialIdAllowNull(long materialId)
        {
            lock (_wells)
            {
                var well = _wells.Values.FirstOrDefault(well =>
                well.Labware?.Material?.MaterialNo == materialId &&
                well.Status is WellStatus.Loaded or WellStatus.Returned);
                return well;
            }
        }
        /// <summary>
        /// 获取指定孔位(Loaded Only)
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public QWell FindWellByMaterialIdLoadOnly(long materialId)
        {
            lock (_wells)
            {
                var well = _wells.Values.FirstOrDefault(well =>
                 well.Labware?.Material?.MaterialNo == materialId &&
                 well.Status is WellStatus.Loaded);
                return well ?? throw new KeyNotFoundException($"未找到包含材料 {materialId} 的孔位");
            }
        }
        /// <summary>
        /// 获取指定孔位(Loaded Only) (允许为空)
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public QWell? FindWellByMaterialIdLoadOnlyAllowNull(long materialId)
        {
            lock (_wells)
            {
                var well = _wells.Values.FirstOrDefault(well =>
                well.Labware?.Material?.MaterialNo == materialId &&
                well.Status is WellStatus.Loaded);
                return well;
            }
        }
        /// <summary>
        /// 按条件查找孔位
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public QWell? FindWell(Func<QWell, bool> predicate)
        {
            lock (_wells)
            {
                return _wells.Values.FirstOrDefault(predicate);
            }
        }

        public QWell FindFirstWellWithMaterial()
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                           w.Status is WellStatus.Loaded || w.Status is WellStatus.Returned)
               .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstWellWithMaterial)}未找到可以取用的孔位");
            }
        }

        /// <summary>
        /// 获取一个未取出孔位
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindFirstLoadWellWithMaterial()
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                           w.Status is WellStatus.Loaded)
               .OrderBy(w => w.Label)
               .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstLoadWellWithMaterial)}未找到可以取用的孔位");
            }
        }
        /// <summary>
        /// 获取一个未取出孔位(允许为空)
        /// </summary>
        /// <returns></returns>
        public QWell? FindFirstLoadedWellWithMaterialAllowNull()
        {
            lock (_wells)
            {
               var well = _wells.Values
               .Where(w =>
               w.Status is WellStatus.Loaded)
               .OrderBy(w => w.Label)
               .FirstOrDefault();
                return well;
            }
        }
        /// <summary>
        /// 获取一个未取出孔位 倒序查找
        /// </summary>
        /// <returns></returns>
        public QWell? FindFirstLoadWellWithMaterialBackAllowNull()
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
               w.Status is WellStatus.Loaded)
               .OrderByDescending(w => w.Label)
               .FirstOrDefault();
                return well;
            }
        }

        /// <summary>
        /// 获取一个未取出孔位 倒序查找
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindFirstLoadWellWithMaterialBack()
        {
            lock (_wells)
            {
                var well = _wells.Values
                .Where(w =>
                            w.Status is WellStatus.Loaded)
                .OrderByDescending(w => w.Label)
                .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstLoadWellWithMaterialBack)}未找到可以取用的孔位");
            }
        }

        /// <summary>
        ///原取原放
        /// </summary>
        /// <param name="qLabware"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindTakenWell(QLabware qLabware)
        {
            lock (_wells)
            {
                var well = _wells.Values
                .Where(w =>
                            w.Status is WellStatus.Taken && w.Labware == qLabware)
                .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindTakenWell)}未找到指定材料{qLabware}的孔位");
            }
        }
        /// <summary>
        /// 原取原放(允许空)
        /// </summary>
        /// <param name="qLabware"></param>
        /// <returns></returns>
        public QWell? FindTakenWellAllowNull(QLabware qLabware)
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                           w.Status is WellStatus.Taken && w.Labware == qLabware)
               .FirstOrDefault();
                return well;
            }
        }

        /// <summary>
        /// 查找空的或者取出的孔位
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindFirstEmptyOrTakenWell()
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                           w.Status is WellStatus.Empty or WellStatus.Taken)
               .OrderBy(w => w.Label)
               .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstEmptyOrTakenWell)}未找到可以存放的孔位");
            }
        }
        /// <summary>
        /// 查找空的或者取出的孔位(允许空)
        /// </summary>
        /// <returns></returns>
        public QWell? FindFirstEmptyOrTakenWellAllowNull()
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                           w.Status is WellStatus.Empty or WellStatus.Taken)
               .OrderBy(w => w.Label)
               .FirstOrDefault();
                return well;
            }
        }

        /// <summary>
        /// 查找空的孔位
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindFirstEmptyWell()
        {
            lock (_wells)
            {
                var well = _wells.Values
                .Where(w =>
                            w.Status is WellStatus.Empty)
                .OrderBy(w => w.Label)
                .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstEmptyWellBack)}未找到可以存放的孔位");
            }
        }

        /// <summary>
        /// 查找空的孔位 倒序
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindFirstEmptyWellBack()
        {
            lock (_wells)
            {
                var well = _wells.Values
                .Where(w =>
                w.Status is WellStatus.Empty)
                .OrderByDescending(w => w.Label)
                .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstEmptyWellBack)}未找到可以存放的孔位");
            }
        }


        /// <summary>
        /// 查找取出的孔位
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindFirstTakenWell()
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                           w.Status is WellStatus.Taken)
               .OrderBy(w => w.Label)
               .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstTakenWell)}未找到已取出可以存放的孔位");
            }
        }
        /// <summary>
        /// 查找取出的孔位 倒序
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindFirstTakenWellBack()
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                           w.Status is WellStatus.Taken)
               .OrderByDescending(w => w.Label)
               .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindFirstTakenWell)}未找到已取出可以存放的孔位");
            }
        }

        /// <summary>
        /// 通过材料ID和和孔位名称 
        /// </summary>
        /// <param name="materialId"></param>
        /// <param name="wellName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QWell FindWellByMaterialIdAndWellName(int materialId, string wellName)
        {
            lock (_wells)
            {
                var well = _wells.Values
               .Where(w =>
                            w.Labware?.Material?.MaterialNo == materialId && w.Label == wellName)
               .OrderBy(w => w.Label)
               .FirstOrDefault();
                return well ?? throw new Exception($"{nameof(FindWellByMaterialIdAndWellName)}未找到指定材料id{materialId}孔位{wellName}");
            }
        }

        /// <summary>
        /// 转换外部孔位状态
        /// </summary>
        /// <param name="wellName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ExternalWellStatus ChangeExternalWellStatus(string wellName)
        {
            lock (_wells)
            {
                var well = _wells[wellName];
                return well.Status switch
                {
                    WellStatus.Empty => ExternalWellStatus.Idle,
                    WellStatus.Returned => ExternalWellStatus.Used,
                    WellStatus.Loaded => ExternalWellStatus.Available,
                    WellStatus.Taken => ExternalWellStatus.Idle,
                    _ => throw new Exception($"{nameof(ChangeExternalWellStatus)}未找到指定孔位{wellName}的状态")
                };
            }
        }


        /// <summary>
        /// 获取所有孔位的状态信息列表
        /// </summary>
        /// <returns>包含每个孔位名称、状态、样品ID、耗材ID的 WellInfo 列表</returns>
        public List<WellInfo> GetWellStatuses()
        {
            lock (_wells)
            {
                var list = new List<WellInfo>();
                foreach (var kvp in _wells)
                {
                    var well = kvp.Value;
                    var wellInfo = new WellInfo
                    {
                        WellName = well.Label,
                        WellStatus = ChangeExternalWellStatus(well.Label),
                        MaterialId = well.Labware?.Material?.MaterialNo ?? 0,
                    };
                    list.Add(wellInfo);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取托盘信息
        /// </summary>
        /// <returns></returns>
        public LabTrayInfo GetLabTrayInfo()
        {
            lock (_wells) 
            {
                var info = new LabTrayInfo
                {
                    WellInfos = GetWellStatuses(),
                    LabTrayId = LabTrayId,
                    LabTrayCategory = LabTrayCategory,
                    LabTrayCode = LabTrayCode,
                    LabTrayName = LabTrayName,
                    IsFinalTray = IsFinalTray,
                };
                return info;
            }
        }

        /// <summary>
        /// 获取托盘建模信息
        /// </summary>
        /// <returns></returns>
        public LabTrayConfiguration GetLabTrayConfig()
        {
            lock (_wells) 
            {
                var config = new LabTrayConfiguration
                {
                    LabTrayId = LabTrayId,
                    LabTrayName = LabTrayName,
                    LabTrayCategory = LabTrayCategory,
                    LabTrayCode = LabTrayCode,
                    TotalCols = GetTotalCols(),
                    TotalRows = GetTotalRows(),
                    WellInfos = [.. _wells.OrderBy(p => p.Key).Select(p => new WellInfo
                {
                    MaterialId = p.Value.Labware?.Material?.MaterialNo ?? 0,
                    WellName = p.Value.Label,
                    WellStatus = ChangeExternalWellStatus(p.Value.Label),
                })],
                    TraySegmentRegions = [.. Regions/*.Where(p => !p.VirtualRegion)*/.Select(p => new TraySegmentRegion
                {
                    RegionCols = p.Cols,
                    RegionRows = p.Rows,
                    RegionWellInfos = [.. p.GetRegionWells().OrderBy(p => p.Label).Select(p => new WellInfo
                    {
                        MaterialId = p.Labware?.Material?.MaterialNo ?? 0,
                        WellName = p.Label,
                        WellStatus = ChangeExternalWellStatus(p.Label),
                    })],
                    TrayRegionId = p.TrayRegionId,
                    TrayRegionName = p.TrayRegionName,
                    TrayRegionType = p.TrayRegionType,
                })],
                };
                return config;
            }
        }

        /// <summary>
        /// 获取器皿信息
        /// </summary>
        /// <returns></returns>
        public LabwareInfo[] GetLabwareInfos()
        {
            lock (_wells)
            {
                return [.. _wells.Values
                .OrderBy(p=>p.Label)
                .Where(p => p.Labware != null)
                .Select(p => p.Labware.GetLabwareInfo())];
            }
        }

        /// <summary>
        /// 获取托盘的初始绑定信息
        /// </summary>
        /// <returns></returns>

        public TrayInitialBindingInfo GetTrayInitialBindingInfo()
        {
            return InitialBindingInfo;
        }

        /// <summary>
        /// 通过区域id查找指定区域
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public TrayRegion GetRegion(long regionId)
        {
            lock (_wells)
            {
                if (Regions.Count == 1)
                    return Regions[0];
                if (Regions.Count > 1)
                {
                    var region = Regions.FirstOrDefault(r => r.TrayRegionId == regionId) ?? throw new Exception($"{nameof(GetRegion)}未找到指定ID{regionId}的孔区");
                    return region;
                }
                throw new Exception($"(Regions.Count == 0),{nameof(GetRegion)}未找到指定ID{regionId}的孔区");
            }
        }

        /// <summary>
        /// 通过区域id和材料id查找一个孔位
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public QWell FindWell(long regionId, long materialId)
        {
            lock (_wells)
            {
                return GetRegion(regionId).GetWellByMaterialId(materialId);
            }
        }

        public object Clone()
        {
            var clone = new LabTray
            {
                LabTrayCategory = LabTrayCategory,
                LabTrayName = LabTrayName,
                LabTrayCode = LabTrayCode,
                ParameterId = Guid.NewGuid(),
                RowFirst = RowFirst,
                LabTrayDescription = LabTrayDescription,
                LabwareName = LabwareName,
                LabTrayId = SnowflakeIdGenerator.Instance.GenerateYitId(),
                Regions = [.. Regions.Select(r => (TrayRegion)r.Clone())]
            };
            return clone;
        }

        public void Log(string message)
        {
            //托盘名称 + message
            _logger?.LogInformation($"{LabTrayName}:{message}");
        }
    }




}
