namespace ParkingGarage;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    private FlowLayoutPanel _menuPanel = null!;

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
        _menuPanel = new FlowLayoutPanel();
        SuspendLayout();

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.WhiteSmoke;
        lblTitle.Margin = new Padding(0, 0, 0, 28);
        lblTitle.Text = "Ondergrondse parking — simulatie";

        btnStart.AutoSize = true;
        btnStart.BackColor = AppColors.PanelControl;
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
        btnExit.BackColor = AppColors.PanelControl;
        btnExit.FlatStyle = FlatStyle.Flat;
        btnExit.FlatAppearance.BorderColor = Color.Gainsboro;
        btnExit.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        btnExit.ForeColor = Color.WhiteSmoke;
        btnExit.Margin = new Padding(8);
        btnExit.Padding = new Padding(32, 12, 32, 12);
        btnExit.Text = "stop simulatie";
        btnExit.UseVisualStyleBackColor = false;
        btnExit.Click += BtnExit_Click;

        _menuPanel.AutoSize = true;
        _menuPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _menuPanel.BackColor = AppColors.FieldBackground;
        _menuPanel.FlowDirection = FlowDirection.TopDown;
        _menuPanel.WrapContents = false;
        _menuPanel.Controls.Add(lblTitle);
        _menuPanel.Controls.Add(btnStart);
        _menuPanel.Controls.Add(btnExit);
        _menuPanel.Dock = DockStyle.Fill;
        _menuPanel.Padding = new Padding(40);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppColors.FieldBackground;
        ClientSize = AppLayout.SimulationClientSize;
        Controls.Add(_menuPanel);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Parking — simulatie";
        ResumeLayout(false);
    }
}
