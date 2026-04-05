namespace ParkingGarage;

partial class MainMenuForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var lblTitle = new Label();
        var btnStart = new Button();
        var btnExit = new Button();
        var panel = new FlowLayoutPanel();
        SuspendLayout();

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.WhiteSmoke;
        lblTitle.Margin = new Padding(0, 0, 0, 28);
        lblTitle.Text = "Ondergrondse parking — simulatie";

        btnStart.AutoSize = true;
        btnStart.BackColor = Color.FromArgb(40, 40, 40);
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.FlatAppearance.BorderColor = Color.Gainsboro;
        btnStart.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        btnStart.ForeColor = Color.WhiteSmoke;
        btnStart.Margin = new Padding(8);
        btnStart.Padding = new Padding(32, 12, 32, 12);
        btnStart.Text = "Start parkeren";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += BtnStart_Click;

        btnExit.AutoSize = true;
        btnExit.BackColor = Color.FromArgb(40, 40, 40);
        btnExit.FlatStyle = FlatStyle.Flat;
        btnExit.FlatAppearance.BorderColor = Color.Gainsboro;
        btnExit.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        btnExit.ForeColor = Color.WhiteSmoke;
        btnExit.Margin = new Padding(8);
        btnExit.Padding = new Padding(32, 12, 32, 12);
        btnExit.Text = "Exit";
        btnExit.UseVisualStyleBackColor = false;
        btnExit.Click += BtnExit_Click;

        panel.AutoSize = true;
        panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        panel.BackColor = Color.Black;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.Controls.Add(lblTitle);
        panel.Controls.Add(btnStart);
        panel.Controls.Add(btnExit);
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(40);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(560, 360);
        Controls.Add(panel);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Parking — menu";
        ResumeLayout(false);
    }
}
