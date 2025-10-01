using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 孔位
    /// </summary>
    public class Well
    {
        public long SampleId { get; set; }
        /// <summary>
        /// 孔位Id
        /// </summary>
        public int WellId { get; set; }
        /// <summary>
        /// 孔位编号
        /// </summary>
        public string WellName { get; set; }
        /// <summary>
        /// 托盘Id
        /// </summary>
        public Guid  LabWareId { get; set; }

        public WellStatus WellStatus { get; set; } = WellStatus.Empty;
        /// <summary>
        /// 位置
        /// </summary>
        public QPosition Pos { get; set; }
    }

    public enum WellStatus
    {
        /// <summary>
        /// 空孔位
        /// </summary>
        Empty = 1,

        /// <summary>
        /// 放入材料，未取出
        /// </summary>
        Loaded = 2,

        /// <summary>
        /// 已取出，材料未返回
        /// </summary>
        Taken = 3,

        /// <summary>
        /// 材料取出后又放回
        /// </summary>
        Returned = 4,
    }
    /// <summary>
    /// 孔位
    /// </summary>
    public class QWell : QData, ISampleStow
    {
        public string LabTrayName { get; set; } = string.Empty;
        public long LabTrayId { get; set; }
        public string LabwareName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public WellStatus Status { get; private set; } = WellStatus.Empty;
        public QPosition Position { get; set; } = new();
        public QLabware  Labware { get; set; }
        public ClawGraspInfo ClawSetting { get; set; } = new ClawGraspInfo();

        /// <summary>
        /// 孔位变更事件
        /// </summary>
        public event Action<QWell>? WellChanged;

        public void SetWell(WellInfo wellInfo)
        {
            switch (wellInfo.WellStatus)
            {
                case ExternalWellStatus.Idle:
                    Status = WellStatus.Empty;
                    break;
                case ExternalWellStatus.Available:
                    Status = WellStatus.Loaded;
                    Labware = new QLabware() 
                    {
                        LabwareName = LabwareName,
                        OwnerLabTrayName = LabTrayName,
                        OwnerLabTrayId = LabTrayId,
                        OwnerLabTrayPositionLabel = Label,
                        Material = new QMaterial(wellInfo.MaterialId) 
                    };
                    break;
                case ExternalWellStatus.Used:
                    Status = WellStatus.Returned;
                    Labware = new QLabware() 
                    {
                        LabwareName = LabwareName,
                        OwnerLabTrayName = LabTrayName,
                        OwnerLabTrayId= LabTrayId,
                        OwnerLabTrayPositionLabel = Label,
                        Material = new QMaterial(wellInfo.MaterialId)
                    };
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        ///  放置材料
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void PlaceMaterial(long materialid)
        {
            if (Status != WellStatus.Empty)
                throw new InvalidOperationException("孔位非空，不能放入材料");

            Labware = new QLabware
            {
                LabwareId = Guid.NewGuid(),
                LabwareName = LabwareName,
                OwnerLabTrayName = LabTrayName,
                OwnerLabTrayId = LabTrayId,
                OwnerLabTrayPositionLabel = Label,
                Material = new QMaterial(materialid)
            };

            Status = WellStatus.Loaded;
        }
        /// <summary>
        /// 放置材料
        /// </summary>
        /// <param name="labware"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void PlaceMaterial(QLabware labware)
        {
            if (Status != WellStatus.Empty || Status != WellStatus.Taken)
                throw new InvalidOperationException("孔位非空，不能放入材料");
            if (Labware != null)
            {
                if (labware?.Material != null)
                {
                    Labware?.PlaceMaterial(labware.Material);
                }
            }
            else
            {
                Labware = labware;
            }
            Status = WellStatus.Loaded;
        }
        /// <summary>
        /// 试管放入材料
        /// </summary>
        /// <param name="materialid"></param>
        public void PlaceMaterialToTube(long materialid)
        {
            if (Status == WellStatus.Empty || Status == WellStatus.Taken)
                throw new InvalidOperationException($"孔位为空{materialid}，不能放入材料");
            if (Labware == null)
                throw new InvalidOperationException($"器皿为空{materialid}，不能放入材料");
            Labware.PlaceMaterial(new QMaterial(materialid));
        }

        /// <summary>
        /// 取出
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public QLabware Take()
        {
            if (Status != WellStatus.Loaded && Status != WellStatus.Returned)
                throw new InvalidOperationException("当前状态不可取出");

            var current = Labware;
            Status = WellStatus.Taken;
            return current;
        }

        public void Return(QLabware labware)
        {
            if (Status != WellStatus.Taken)
                throw new InvalidOperationException("当前状态不允许归还");
            if (labware != Labware)
                throw new InvalidOperationException("归还的器皿不匹配");
            Labware = labware;
            Status = WellStatus.Returned;
        }


        public void Return()
        {
            if (Status != WellStatus.Taken)
                throw new InvalidOperationException("当前状态不允许归还");
           
            Status = WellStatus.Returned;
        }

        public void ClearEmpty()
        {
            Labware = null;
            Status = WellStatus.Empty;
        }

        public override string ToString()
        {
            return $"孔位{LabwareName}，状态{Status}，位置{Position}，器皿{Labware}，抓取信息{ClawSetting}";
        }
    }

    public enum MaterialStatus
    {
        /// <summary> 原始未使用材料 </summary>
        New = 1,

        /// <summary> 材料已使用 </summary>
        Used= 2,

        /// <summary> 留液 </summary>
        Reserved= 3,
    }

    /// <summary>
    /// 实验材料
    /// </summary>
    public class QMaterial : QData
    {
        public QMaterial(long materialNo)
        {
            MaterialNo = materialNo;
            UniqueId = Guid.NewGuid();
        }
        /// <summary>
        /// 材料编号
        /// </summary>
        public long  MaterialNo { get; set; }
        /// <summary>
        /// 材料状态
        /// </summary>
        public MaterialStatus  MaterialStatus { get; set; }
        /// <summary>
        /// 派生材料
        /// </summary>
        public Guid SourceId { get; set; }
        /// <summary>
        /// 材料Id
        /// </summary>
        public Guid UniqueId { get; set; }
        /// <summary>
        /// 孪生材料
        /// </summary>
        public List<QMaterial> Twins { get; set; } = [];

        /// <summary>
        /// 合成材料项
        /// </summary>
        public List<QMaterial> SynthesisMaterials { get; set; } = [];

        /// <summary>
        /// 合成材料
        /// </summary>
        /// <param name="material"></param>
        public void Synthesis(QMaterial material)
        {
            SynthesisMaterials.Add(material);
        }
       

        public override string ToString()
        {
            return $"材料{MaterialNo}，状态{MaterialStatus}，派生{SourceId}，唯一{UniqueId}，孪生{string.Join(",", Twins.Select(t => t.ToString()))}";
        }
    }

    //历史材料记录
    public class MaterialHistoryRecord
    {
        public Guid UniqueId { get; set; }

        public long MaterialNo { get; set; }

        public MaterialStatus MaterialStatus { get; set; }

        public Guid SourceId { get; set; }

        public DateTime CreateTime { get; set; }
    }
    
    /// <summary>
    /// 实验材料器皿
    /// </summary>
    public class QLabware : QData
    {
        public Guid  LabwareId { get; set; }
        /// <summary>
        /// 器皿材料
        /// </summary>
        public QMaterial? Material { get; set; }
        /// <summary>
        /// 器皿从属托盘
        /// </summary>
        public string OwnerLabTrayName { get; set; } = string.Empty;

        /// <summary>
        /// 器皿从属托盘Id
        /// </summary>
        public long OwnerLabTrayId { get; set; }
        /// <summary>
        /// 器皿从属托盘位置
        /// </summary>
        public string OwnerLabTrayPositionLabel { get; set; } = string.Empty;
        /// <summary>
        /// 器皿名称
        /// </summary>
        public string LabwareName { get; set; } = string.Empty;
        /// <summary>
        /// 器皿二维码
        /// </summary>
        public string QrCode { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public List<MaterialHistoryRecord> MaterialHistories { get; set; } = [];


        public void SetQrCode(string qrCode)
        {
            QrCode = qrCode;
        }


        /// <summary>
        /// 放入材料
        /// </summary>
        /// <param name="material"></param>
        public void PlaceMaterial(QMaterial material)
        {
            if (this.Material != null)
            {
                MaterialHistories.Add(new MaterialHistoryRecord
                {
                     MaterialNo = this.Material.MaterialNo,
                     CreateTime = DateTime.Now,
                     MaterialStatus = this.Material.MaterialStatus,
                     UniqueId = this.Material.UniqueId,
                     SourceId = this.Material.SourceId,
                });
            }
            Material = material;
        }


        /// <summary>
        /// 取出原材料
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public QMaterial TakeMaterial()
        {
            return Material ?? throw new InvalidOperationException("无材料可取出");
        }

        /// <summary>
        /// 取出孪生材料
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public QMaterial TakeTwinMaterial()
        {
            if (Material == null)
                throw new InvalidOperationException("无材料器皿可取出");
            var twin = new QMaterial(Material.MaterialNo)
            {
                SourceId = Material.UniqueId,
            };
            Material.Twins.Add(twin);
            return twin;
        }

        /// <summary>
        /// 移除材料
        /// </summary>
        public void RemoveMaterial()
        {
            Material = null;
        }


        public LabwareInfo GetLabwareInfo()
        {
            return new LabwareInfo
            {
                LabwareName = LabwareName,
                QrCode = QrCode,
                OwnerLabTrayId = OwnerLabTrayId,
                OwnerLabTrayPositionLabel = OwnerLabTrayPositionLabel,
                MaterialInfo = new MaterialInfo
                {
                    MaterialNo = Material?.MaterialNo ?? 0,
                }
            };
        }

        public override string ToString()
        {
            return $"器皿{LabwareId} {LabwareName}，材料{Material?.ToString() ?? "无"}，位置{OwnerLabTrayName}.{OwnerLabTrayPositionLabel}";
        }
    }

}
