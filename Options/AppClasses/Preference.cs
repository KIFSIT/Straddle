using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using ClientCommon;
using MTCommon;

namespace Straddle.AppClasses
{
    public class Preference : MTSettings
    {
        #region Instance
        static Preference _instance;
        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static Preference Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new Preference();

                    if ( !File.Exists( MTClientEnvironment.SpecialFiles.Preference ) )
                        Instance.SaveData();

                    Instance.ReadData();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        Preference()
            : base( MTClientEnvironment.SpecialFiles.Preference, typeof( Preference ) )
        {

        }

        public void SaveData()
        {
            base.SaveData( _instance );
        }

        public void ReadData()
        {
            base.ReadData( ref _instance );
        }

        public void ReadData( string path )
        {
            base.SettingFilePath = path;
            ReadData();
        }
        #endregion

        #region MarkaetWatch Colors

        private Color _ActiveBackColor = Color.DarkRed;
        private Color _ActiveForeColor = Color.LightGray;
        private Color _PriceIncreaseForeColor = Color.White;
        private Color _PriceIncreaseBackColor = Color.Blue;
        private Color _PriceDecreaseForeColor = Color.White;
        private Color _PriceDecreaseBackColor = Color.Red;

        [Category( "MarketWatch Settings" )]
        [DisplayName( "Active Script BackColor" )]
        [Description( "BackColor of Active Script in MarketWatch" )]
        [XmlElement( Type = typeof( XmlColor ) )]
        public Color ActiveBackColor
        {
            get
            {
                return _ActiveBackColor;
            }
            set
            {
                _ActiveBackColor = value;
            }
        }

        [Category( "MarketWatch Settings" )]
        [DisplayName( "Active Script ForeColor" )]
        [Description( "ForeColor of Active Script in MarketWatch" )]
        [XmlElement( Type = typeof( XmlColor ) )]
        public Color ActiveForeColor
        {
            get
            {
                return _ActiveForeColor;
            }
            set
            {
                _ActiveForeColor = value;
            }
        }

        [Category( "MarketWatch Settings" )]
        [DisplayName( "Price Increase BackColor" )]
        [Description( "Price BackColor When Price Increase in MarketWatch" )]
        [XmlElement( Type = typeof( XmlColor ) )]
        public Color PriceIncreaseBackColor
        {
            get
            {
                return _PriceIncreaseBackColor;
            }
            set
            {
                _PriceIncreaseBackColor = value;
            }
        }

        [Category( "MarketWatch Settings" )]
        [DisplayName( "Price Increase ForeColor" )]
        [Description( "Price ForeColor When Price Increase in MarketWatch" )]
        [XmlElement( Type = typeof( XmlColor ) )]
        public Color PriceIncreaseForeColor
        {
            get
            {
                return _PriceIncreaseForeColor;
            }
            set
            {
                _PriceIncreaseForeColor = value;
            }
        }

        [Category( "MarketWatch Settings" )]
        [DisplayName( "Price Decrease BackColor" )]
        [Description( "Price BackColor When Price Decrease in MarketWatch" )]
        [XmlElement( Type = typeof( XmlColor ) )]
        public Color PriceDecreaseBackColor
        {
            get
            {
                return _PriceDecreaseBackColor;
            }
            set
            {
                _PriceDecreaseBackColor = value;
            }
        }

        [Category( "MarketWatch Settings" )]
        [DisplayName( "Price Decrease ForeColor" )]
        [Description( "Price ForeColor When Price Decrease in MarketWatch" )]
        [XmlElement( Type = typeof( XmlColor ) )]
        public Color PriceDecreaseForeColor
        {
            get
            {
                return _PriceDecreaseForeColor;
            }
            set
            {
                _PriceDecreaseForeColor = value;
            }
        }

        #endregion
    }
}
