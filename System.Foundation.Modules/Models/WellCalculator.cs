using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.TubeRacks;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Models
{
 
    public class WellCalculator
    {
        public static List<Well> CalculateWells(TubeRackData rackData)
        {
            var wells = new List<Well>();
            int wellId = 1;

            if (rackData.Rows <= 0
                || rackData.Cols <= 0
                || string.IsNullOrEmpty(rackData.RowLable)
                || string.IsNullOrEmpty(rackData.ColLable))
            {
                return wells;
            }

            string[] rowLabels = rackData.RowLable.Split(',');
            string[] colLabels = rackData.ColLable.Split(',');

            if (rowLabels.Length != rackData.Rows || colLabels.Length != rackData.Cols)
            {
                return wells;
            }
            var guid = Guid.NewGuid();
            if (rackData.Rowfirst)
            {
                for (int row = 0; row < rackData.Rows; row++)
                {
                    for (int col = 0; col < rackData.Cols; col++)
                    {
                        float posX = rackData.StartPosition.X + col * rackData.SpaceCol;
                        float posY = rackData.StartPosition.Y + row * rackData.SpaceRow;

                        string rowLabel = rowLabels[row];
                        string colLabel = colLabels[col];

                        wells.Add(CreateWell(guid, wellId++, rowLabel, colLabel, posX, posY, rackData));
                    }
                }
            }
            else
            {
                for (int col = 0; col < rackData.Cols; col++)
                {
                    for (int row = 0; row < rackData.Rows; row++)
                    {
                        float posX = rackData.StartPosition.X + col * rackData.SpaceCol;
                        float posY = rackData.StartPosition.Y + row * rackData.SpaceRow;

                        string rowLabel = rowLabels[row];
                        string colLabel = colLabels[col];

                        wells.Add(CreateWell(guid, wellId++, rowLabel, colLabel, posX, posY, rackData));
                    }
                }
            }
            return wells;
        }

        private static Well CreateWell(Guid labwareId,int id, string rowLabel, string colLabel, float x, float y, TubeRackData data)
        {
            return new Well
            {
                WellId = id,
                WellName = $"{rowLabel}{colLabel}",
                LabWareId = labwareId,
                Pos = new QPosition
                {
                    X = x,
                    Y = y,
                    Angle = data.StartPosition.Angle,
                    Depth = data.StartPosition.Depth,
                    Z = data.StartPosition.Z,
                    Z2 = data.StartPosition.Z2,
                    ZPutGetOffset = data.StartPosition.ZPutGetOffset,
                }
            };
        }

    }
}
