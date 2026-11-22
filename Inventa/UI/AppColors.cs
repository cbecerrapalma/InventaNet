using System;
using System.Drawing;
using System.Windows.Forms;

namespace Inventa.UI
{
    /// <summary>
    /// Paleta de colores consistente para toda la aplicación
    /// Basada en Fluent Design System de Microsoft
    /// </summary>
    public static class AppColors
    {
        // Colores primarios
        public static readonly Color Primary = ColorTranslator.FromHtml("#0078D7");
        public static readonly Color PrimaryHover = ColorTranslator.FromHtml("#106EBE");
        public static readonly Color PrimaryPressed = ColorTranslator.FromHtml("#005A9E");
        public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#E5F3FF");
        public static readonly Color MenuStripBackColor = ColorTranslator.FromHtml("#1E3C72");

        // Colores semánticos
        public static readonly Color Success = ColorTranslator.FromHtml("#107C10");
        public static readonly Color SuccessLight = ColorTranslator.FromHtml("#DFF6DD");
        public static readonly Color Warning = ColorTranslator.FromHtml("#CA5010");
        public static readonly Color WarningLight = ColorTranslator.FromHtml("#FFF4CE");
        public static readonly Color Error = ColorTranslator.FromHtml("#D13438");
        public static readonly Color ErrorLight = ColorTranslator.FromHtml("#FDE7E9");
        public static readonly Color Info = ColorTranslator.FromHtml("#0078D7");
        public static readonly Color InfoLight = ColorTranslator.FromHtml("#E7F3FF");

        // Colores de borde
        public static readonly Color Border = ColorTranslator.FromHtml("#8A8886");
        public static readonly Color BorderLight = ColorTranslator.FromHtml("#EDEBE9");
        public static readonly Color BorderDark = ColorTranslator.FromHtml("#323130");

        // Colores de fondo
        public static readonly Color Background = ColorTranslator.FromHtml("#F3F2F1");
        public static readonly Color BackgroundLight = ColorTranslator.FromHtml("#FAFAFA");
        public static readonly Color BackgroundDark = ColorTranslator.FromHtml("#E8E8E8");
        public static readonly Color Surface = Color.White;
        public static readonly Color InactiveRowBackground = ColorTranslator.FromHtml("#F0F0F0");

        // Colores de texto
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#323130");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#605E5C");
        public static readonly Color TextDisabled = ColorTranslator.FromHtml("#A19F9D");
        public static readonly Color TextPlaceholder = ColorTranslator.FromHtml("#C8C6C4");
    }

    /// <summary>
    /// Utilidades para mejorar controles WinForms
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// Aplica estilo profesional a un DataGridView
        /// </summary>
        public static void ApplyModernStyle(this DataGridView dgv)
        {
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = AppColors.BorderLight;
            dgv.EnableHeadersVisualStyles = false;

            // Estilo del header
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Background;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8);
            dgv.ColumnHeadersHeight = 40;

            // Estilo de las filas
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = AppColors.TextSecondary;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.SelectionBackColor = AppColors.PrimaryLight;
            dgv.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            dgv.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            dgv.RowTemplate.Height = 36;

            // Estilo de fila alternada
            dgv.AlternatingRowsDefaultCellStyle.BackColor = AppColors.BackgroundLight;
        }

        /// <summary>
        /// Aplica estilo profesional a un botón
        /// </summary>
        public static void ApplyPrimaryStyle(this Button btn)
        {
            btn.BackColor = AppColors.Primary;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Padding = new Padding(12, 6, 12, 6);

            // Eventos hover
            btn.MouseEnter += (s, e) => btn.BackColor = AppColors.PrimaryHover;
            btn.MouseLeave += (s, e) => btn.BackColor = AppColors.Primary;
            btn.MouseDown += (s, e) => btn.BackColor = AppColors.PrimaryPressed;
            btn.MouseUp += (s, e) => btn.BackColor = AppColors.PrimaryHover;
        }

        /// <summary>
        /// Aplica estilo secundario a un botón
        /// </summary>
        public static void ApplySecondaryStyle(this Button btn)
        {
            btn.BackColor = Color.White;
            btn.ForeColor = AppColors.TextPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = AppColors.Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = new Font("Segoe UI", 9F);
            btn.Cursor = Cursors.Hand;
            btn.Padding = new Padding(12, 6, 12, 6);

            // Eventos hover
            btn.MouseEnter += (s, e) => btn.BackColor = AppColors.BackgroundLight;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.White;
        }

        /// <summary>
        /// Aplica estilo de peligro a un botón
        /// </summary>
        public static void ApplyDangerStyle(this Button btn)
        {
            btn.BackColor = AppColors.Error;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Padding = new Padding(12, 6, 12, 6);
        }

        /// <summary>
        /// Crea un panel con estilo de card
        /// </summary>
        public static Panel CreateCard(string title = null)
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            if (!string.IsNullOrEmpty(title))
            {
                var lblTitle = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = AppColors.TextPrimary,
                    AutoSize = true,
                    Location = new Point(15, 15)
                };
                panel.Controls.Add(lblTitle);
            }

            return panel;
        }

        /// <summary>
        /// Aplica placeholder a un TextBox
        /// </summary>
        public static void SetPlaceholder(this TextBox txt, string placeholder)
        {
            txt.Text = placeholder;
            txt.ForeColor = AppColors.TextPlaceholder;

            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = AppColors.TextPrimary;
                }
            };

            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = AppColors.TextPlaceholder;
                }
            };
        }

        /// <summary>
        /// Aplica estilo moderno a un GroupBox
        /// </summary>
        public static void ApplyModernStyle(this GroupBox grp)
        {
            grp.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grp.ForeColor = AppColors.TextPrimary;
            grp.Padding = new Padding(12);
        }

        /// <summary>
        /// Aplica estilo moderno a un ListView
        /// </summary>
        public static void ApplyModernStyle(this ListView lv)
        {
            lv.View = View.Details;
            lv.FullRowSelect = true;
            lv.GridLines = false;
            lv.BorderStyle = BorderStyle.None;
            lv.Font = new Font("Segoe UI", 9F);
            lv.ForeColor = AppColors.TextSecondary;
            lv.BackColor = Color.White;
        }
    }
    
}
