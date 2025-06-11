using System;
using System.Windows.Forms;

namespace Multi_Thread_Elevator
{
    public partial class FormConfiguracion : Form
    {
        public int CantidadEdificios { get; private set; }
        public int AscensoresPorEdificio { get; private set; }
        public int CantidadPisos { get; private set; }

        public FormConfiguracion()
        {
            InitializeComponent();

            txtEdificios.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    txtAscensores.Focus();
                }
            };

            txtAscensores.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    txtPisos.Focus();
                }
            };

            txtPisos.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnAceptar.PerformClick(); // llama a la validación
                }
            };
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtEdificios.Text, out int edificios) &&
                int.TryParse(txtAscensores.Text, out int ascensores) &&
                int.TryParse(txtPisos.Text, out int pisos) &&
                edificios >= 1 && edificios <= 5 &&
                ascensores >= 1 && ascensores <= 3 &&
                pisos >= 3 && pisos <= 8)
            {
                CantidadEdificios = edificios;
                AscensoresPorEdificio = ascensores;
                CantidadPisos = pisos;
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Valores inválidos. Verifique los rangos permitidos:\n- Edificios (1-5)\n- Ascensores (1-3)\n- Pisos (3-8)");
            }
        }
    }
}