using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

public partial class ProgressBarEx : ProgressBar
{
    Font fontDefault = new Font("微软雅黑", 12, FontStyle.Regular, GraphicsUnit.Pixel);
    Brush b = new SolidBrush(Color.Black);
    private bool processing = false;
    private string defaultTips = "";

    public ProgressBarEx()
    {
        this.SetStyle(ControlStyles.UserPaint, true);
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    }
    /// <summary>
    /// 是否正在加载进度
    /// </summary>
    public bool Processing
    {
        get { return processing; }
        set
        {
            processing = value;
            if (processing)
                ForeColor = Color.MediumSeaGreen;
            else
                ForeColor = Color.LightGray;
        }
    }
    /// <summary>
    /// 默认显示文字
    /// </summary>
    public string DefaultTips
    {
        get { return defaultTips; }
        set
        {
            defaultTips = value;
        }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        SolidBrush brush = null;
        Rectangle bounds = new Rectangle(0, 0, base.Width, base.Height);
        Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);
        Graphics gbmp = Graphics.FromImage(bmp);
        gbmp.FillRectangle(new SolidBrush(this.BackColor), 1, 1, bounds.Width - 2, bounds.Height - 2);
        bounds.Height -= 4;
        bounds.Width = ((int)(bounds.Width * (((double)base.Value) / ((double)base.Maximum)))) - 4;
        brush = new SolidBrush(this.ForeColor);
        gbmp.FillRectangle(brush, 2, 2, bounds.Width, bounds.Height);
        brush.Dispose();
        string num = "";
        if (processing)
            num = this.Value.ToString() + "%";
        else
            num = defaultTips;
        var fsize = gbmp.MeasureString(num, fontDefault);
        gbmp.DrawString(num, fontDefault, b, new PointF(Width / 2 - fsize.Width / 2, Height / 2 - fsize.Height / 2));
        e.Graphics.DrawImage(bmp, 0, 0);
        gbmp.Dispose();
        bmp.Dispose();
    }
}