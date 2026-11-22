using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Inventa.Forms.Compras
{
    public partial class frm_OrdenCompra : Form
    {
        public frm_OrdenCompra()
        {
            InitializeComponent();

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);
        }
    }
}
