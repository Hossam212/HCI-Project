using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using TUIO;
using System.IO;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Reflection;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Net.Mail;
using static System.Windows.Forms.LinkLabel;


public class TuioDemo : Form, TuioListener
{
    string macmessage;
    private TuioClient client;
    string Namespiceficstudent;
    private Dictionary<long, TuioObject> objectList;
    private Dictionary<long, TuioCursor> cursorList;
    private Dictionary<long, TuioBlob> blobList;
    bool Showspiceficstudent = false;
    Brush startBrush = Brushes.Green;
    Brush showspecificplayerresults = Brushes.Indigo;
    Brush endBrush = Brushes.Red;
    private int cgermany = 0;
    private int cspain = 0;
    private int cegypt = 0;
    private int correctct = 0;
    private int score = 0;
    private int mistakes = 0;
    public static int width, height;
    private int window_width = 1280;
    private int window_height = 700;
    private int window_left = 0;
    private int window_top = 0;
    private int screen_width = Screen.PrimaryScreen.Bounds.Width;
    private int screen_height = Screen.PrimaryScreen.Bounds.Height;
    private Thread listenerThread;
    public string serverIP = "MohammedAdnan";
    private bool isRunning = false; // Flag to manage application state
    private int menuSize1 = 400;
    private int menuSize2 = 400;
    private int menuSize3 = 400;
    private int menuSize4 = 400;
    private bool fullscreen;
    private bool verbose;
    Font font = new Font("Arial", 10.0f);
    SolidBrush fntBrush = new SolidBrush(Color.White);
    SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
    SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
    SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
    SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
    Brush FntMenuClr = Brushes.White;
    Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);
    private string objectImagePath;
    private string backgroundImagePath;
    private string markimagepath;
    private string crossimagepath;
    TcpClient client1;
    NetworkStream stream;
    bool isLogin = false;
    string[] parts;
    private bool isTeacherLogin = false;
    private List<string> studentRecords = new List<string>();
    private List<string> MyRecords = new List<string>();

    private bool isShow = false;
    private bool isPinch = false;
    string playername;
    bool showmyresuts = false;

    private class StudentRecord
    {
        public string MacAddress { get; set; }
        public int Score { get; set; }
        public int Mistakes { get; set; }
    }


    public TuioDemo(int port)
    {
        verbose = false;
        fullscreen = false;
        width = window_width;
        height = window_height;

        this.MaximizeBox = false; // Disable the maximize button
        this.MinimizeBox = true;

        this.ClientSize = new System.Drawing.Size(width, height);
        this.Name = "TuioDemo";
        this.Text = "TuioDemo";

        this.Closing += new CancelEventHandler(Form_Closing);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);

        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint |
                        ControlStyles.DoubleBuffer, true);

        objectList = new Dictionary<long, TuioObject>(128);
        cursorList = new Dictionary<long, TuioCursor>(128);
        blobList = new Dictionary<long, TuioBlob>(128);

        client = new TuioClient(port);
        client.addTuioListener(this);
        client.connect();

        StartConnection();

    }

    private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {

        if (e.KeyData == Keys.F1)
        {
            if (fullscreen == false)
            {

                width = screen_width;
                height = screen_height;

                window_left = this.Left;
                window_top = this.Top;

                this.FormBorderStyle = FormBorderStyle.None;
                this.Left = 0;
                this.Top = 0;
                this.Width = screen_width;
                this.Height = screen_height;

                fullscreen = true;
            }
            else
            {

                width = window_width;
                height = window_height;

                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.Left = window_left;
                this.Top = window_top;
                this.Width = window_width;
                this.Height = window_height;

                fullscreen = false;
            }
        }
        else if (e.KeyData == Keys.Escape)
        {
            stream.Close();
            client1.Close();
            this.Close();

        }
        else if (e.KeyData == Keys.V)
        {
            verbose = !verbose;
        }

    }

    private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        client.removeTuioListener(this);

        client.disconnect();

        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Abort(); // Stop the listening thread
        }
        if (isRunning == true)
        {
            WriteScoreToFile(macmessage, score, mistakes, cegypt, cgermany, cspain);
            // Optionally, you can display a message confirming the save
            MessageBox.Show("Game data saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        stream?.Close();

        System.Environment.Exit(0);
    }

    public void addTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Add(o.SessionID, o);
        }
        if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
    }

    public void updateTuioObject(TuioObject o)
    {

        if (verbose) Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
    }

    public void removeTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Remove(o.SessionID);
        }
        if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
    }

    public void addTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList.Add(c.SessionID, c);
        }
        if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
    }

    public void updateTuioCursor(TuioCursor c)
    {
        if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);
    }

    public void removeTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList.Remove(c.SessionID);
        }
        if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
    }

    public void addTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList.Add(b.SessionID, b);
        }
        if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);
    }

    public void updateTuioBlob(TuioBlob b)
    {

        if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);
    }

    public void removeTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList.Remove(b.SessionID);
        }
        if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
    }

    public void refresh(TuioTime frameTime)
    {
        Invalidate();
    }

    private void showText(Graphics g)
    {
        // Set color for the score and mistake boxes
        SolidBrush lightGrayBrush = new SolidBrush(Color.LightGray);
        SolidBrush blackBrush = new SolidBrush(Color.Black);

        // Box dimensions and positioning for score
        int boxWidth = 120;
        int boxHeight = 40;
        int x = 10;
        int y = 10;

        // Draw the score box
        g.FillRectangle(lightGrayBrush, x, y, boxWidth, boxHeight);

        // Box dimensions and positioning for mistakes
        int mistakesX = 10;
        int mistakesY = 60;
        int mistakesBoxWidth = 170;

        // Draw the mistakes box
        g.FillRectangle(lightGrayBrush, mistakesX, mistakesY, mistakesBoxWidth, boxHeight);

        // Set font for drawing the text
        Font font = new Font("Arial", 20);
        g.DrawString("Score: " + score, font, blackBrush, new PointF(x + 10, y + 10));
        g.DrawString("Mistakes: " + mistakes, font, blackBrush, new PointF(mistakesX + 10, mistakesY + 10));
    }

    private async Task ActivateStartMenuOption(int flag)
    {
        await Task.Delay(2000);
        if (flag == 1)
        {
            isRunning = true;
            showmyresuts = false;

        }
        else if (flag == 2)
        {
            stream.Close();
            client1.Close();
            this.Close();
        }
        if (flag == 3)
        {
            isRunning = false;
            showmyresuts = true;
        }
        else if (flag == 4)
        {
            showmyresuts = false;
            isRunning = false;
        }
    }

    private async Task ActivateViewStudentRecords(int flag)
    {
        await Task.Delay(2000);
        if (flag == 1)
        {
            isShow = true;
        }
        else if (flag == 2)
        {
            stream.Close();
            client1.Close();
            this.Close();
        }
        else if (flag == 3)
        {
            isShow = false;
        }
        else if (flag == 4)
        {
            showmyresuts = false;
        }
    }

    protected void drawmenu(PaintEventArgs prevent, Graphics g)
    {
        Invalidate();
        g.DrawString("Student Menu", new Font("Arial", 25, FontStyle.Bold), Brushes.White, new PointF(480, 20));
        // Draw circle background
        g.FillEllipse(bgrBrush, 400, 100, 400, 400);

        // Define angles for each section
        float startAngle = 270; // Starting angle for the first section
        float sweepAngle = 90;  // Angle for each section

        // Draw sections
        g.FillPie(startBrush, 400, 100, menuSize1, menuSize1, startAngle, sweepAngle); // "Start" section
        startAngle += sweepAngle; // Move to next section

        g.FillPie(showspecificplayerresults, 400, 100, menuSize2, menuSize2, startAngle, sweepAngle); // "Middle" section
        startAngle += sweepAngle; // Move to next section

        g.FillPie(showspecificplayerresults, 400, 100, menuSize3, menuSize3, startAngle, sweepAngle); // "End" section
        startAngle += sweepAngle; // Move to next section

        g.FillPie(endBrush, 400, 100, menuSize4, menuSize4, startAngle, sweepAngle); // "Another" section

        // Draw labels for each section
        g.DrawString("Start", new Font("Arial", 18), FntMenuClr, new PointF(690, 200));
        g.DrawString("My results", new Font("Arial", 18), FntMenuClr, new PointF(557, 430));
        g.DrawString("Exit", new Font("Arial", 18), FntMenuClr, new PointF(445, 210));

        // Draw inner circle to create an empty effect
        g.FillEllipse(bgrBrush, 500, 200, 200, 200); // Adjust size and position as needed
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Getting the graphics object
        Graphics g = pevent.Graphics;
        g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));

        if (isRunning && isLogin)
        {
            backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "background.jpg");
            markimagepath = Path.Combine(Environment.CurrentDirectory, "right.png");
            crossimagepath = Path.Combine(Environment.CurrentDirectory, "cross.png");

            Image mark = Image.FromFile(markimagepath);
            Image cross = Image.FromFile(crossimagepath);
            // Draw background image without rotation
            if (File.Exists(backgroundImagePath))
            {
                using (Image bgImage = Image.FromFile(backgroundImagePath))
                {
                    g.DrawImage(bgImage, new Rectangle(new Point(0, 0), new Size(width, height)));
                }
            }
            else
            {
                Console.WriteLine($"Background image not found: {backgroundImagePath}");
            }
            if (cegypt == 1)
            {
                Image country = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "egypt.png"));
                g.DrawImage(country, new Rectangle(new Point(660, 453), new Size(height / 3, height / 4)));
            }
            if (cgermany == 1)
            {
                Image country = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "germany.png"));
                g.DrawImage(country, new Rectangle(new Point(410, 60), new Size(height / 4, height / 4)));
            }
            if (cspain == 1)
            {
                Image country = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "spain.png"));
                g.DrawImage(country, new Rectangle(new Point(190, 165), new Size(height / 3, height / 3)));
            }
            // We call the score and mistake counter here
            showText(g);

            // Draw the cursor path
            if (cursorList.Count > 0)
            {
                lock (cursorList)
                {
                    foreach (TuioCursor tcur in cursorList.Values)
                    {
                        List<TuioPoint> path = tcur.Path;
                        TuioPoint current_point = path[0];

                        for (int i = 0; i < path.Count; i++)
                        {
                            TuioPoint next_point = path[i];
                            g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height),
                                next_point.getScreenX(width), next_point.getScreenY(height));
                            current_point = next_point;
                        }

                        g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100,
                            current_point.getScreenY(height) - height / 100, height / 50, height / 50);
                        g.DrawString(tcur.CursorID + "", font, fntBrush,
                            new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
                    }
                }
            }

            // Draw the objects
            if (objectList.Count > 0)
            {
                lock (objectList)
                {
                    foreach (TuioObject tobj in objectList.Values)
                    {
                        int ox = tobj.getScreenX(width);
                        int oy = tobj.getScreenY(height);
                        int size = height / 4;
                        bool isCorrect = false; // To track if the object is correctly placed
                        bool isWrong = false;

                        g.TranslateTransform(ox, oy);
                        g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-ox, -oy);

                        // Check if object is in correct position based on its SymbolID
                        switch (tobj.SymbolID)
                        {
                            case 4:  // Germany case
                                if (cgermany == 0)
                                {
                                    objectImagePath = Path.Combine(Environment.CurrentDirectory, "germany.png");
                                    isCorrect = (ox >= 450 && ox <= 550 && oy < 155 && ((tobj.AngleDegrees >= 350 && tobj.AngleDegrees <= 360) || (tobj.AngleDegrees >= 0 && tobj.AngleDegrees <= 10)));// Example coordinates
                                    if ((ox >= 250 && ox <= 360 && oy < 300 && oy > 200) || (ox >= 750 && ox <= 800 && oy > 500 && oy < 600))
                                    {
                                        mistakes += 5;
                                        isWrong = true;
                                    }
                                }
                                break;
                            case 3:  // Spain case
                                if (cspain == 0)
                                {
                                    objectImagePath = Path.Combine(Environment.CurrentDirectory, "spain.png");
                                    isCorrect = (ox >= 250 && ox <= 360 && oy > 200 && oy < 300 && ((tobj.AngleDegrees >= 350 && tobj.AngleDegrees <= 360) || (tobj.AngleDegrees >= 0 && tobj.AngleDegrees <= 10))); // Example coordinates
                                    if ((ox >= 450 && ox <= 550 && oy < 155) || (ox >= 750 && ox <= 800 && oy > 500 && oy < 600))
                                    {
                                        mistakes += 5;
                                        isWrong = true;
                                    }
                                }
                                break;
                            case 0:  // Egypt case
                                if (cegypt == 0)
                                {
                                    objectImagePath = Path.Combine(Environment.CurrentDirectory, "egypt.png");
                                    isCorrect = (ox >= 750 && ox <= 800 && oy > 500 && oy < 600 && ((tobj.AngleDegrees >= 350 && tobj.AngleDegrees <= 360) || (tobj.AngleDegrees >= 0 && tobj.AngleDegrees <= 10)));  // Example coordinates
                                    if ((ox >= 250 && ox <= 360 && oy < 300 && oy > 200) || (ox >= 450 && ox <= 550 && oy < 155))
                                    {
                                        mistakes += 5;
                                        isWrong = true;
                                    }
                                }
                                break;
                            case 1:
                                _ = ActivateStartMenuOption(4);
                                break;
                            default:
                                // Use default rectangle for other 
                                g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                                g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
                                continue;
                        }

                        if (isCorrect && tobj.SymbolID == 0)
                        {
                            score += 5;
                            correctct++;
                            cegypt = 1;
                            SendCountryName(tobj);
                        }
                        if (isCorrect && tobj.SymbolID == 3)
                        {
                            score += 5;
                            correctct++;
                            cspain = 1;
                            SendCountryName(tobj);
                        }
                        if (isCorrect && tobj.SymbolID == 4)
                        {
                            score += 5;
                            correctct++;
                            cgermany = 1;
                            SendCountryName(tobj);
                        }

                        // Check if object is placed correctly and draw the appropriate mark or cross
                        try
                        {
                            // Draw object image with rotation
                            if (File.Exists(objectImagePath))
                            {
                                using (Image objectImage = Image.FromFile(objectImagePath))
                                {
                                    // Save the current state of the graphics object
                                    GraphicsState state = g.Save();

                                    // Apply transformations for rotation
                                    g.TranslateTransform(ox, oy);
                                    g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                                    g.TranslateTransform(-ox, -oy);

                                    // Draw the rotated object
                                    g.DrawImage(objectImage, new Rectangle(ox - size / 2, oy - size / 2, size, size));

                                    // Restore the graphics state
                                    g.Restore(state);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Object image not found: {objectImagePath}");
                                // Fall back to drawing a rectangle
                                //g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                            }

                            // Draw the mark or cross based on correctness
                            if (isCorrect)
                            {
                                g.DrawImage(mark, new Rectangle(ox - size / 4, oy - size / 4, size / 2, size / 2));
                            }
                            if (isWrong)
                            {
                                g.DrawImage(cross, new Rectangle(ox - size / 4, oy - size / 4, size / 2, size / 2));
                                isWrong = false;
                            }
                        }
                        catch
                        {
                            // Handle exceptions (e.g., image not found or drawing error)
                        }
                    }
                }
            }

            // Draw the blobs
            if (blobList.Count > 0)
            {
                lock (blobList)
                {
                    foreach (TuioBlob tblb in blobList.Values)
                    {
                        int bx = tblb.getScreenX(width);
                        int by = tblb.getScreenY(height);
                        float bw = tblb.Width * width;
                        float bh = tblb.Height * height;

                        g.TranslateTransform(bx, by);
                        g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-bx, -by);

                        g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

                        g.TranslateTransform(bx, by);
                        g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-bx, -by);

                        g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
                    }
                }
            }
        }
        else if (isTeacherLogin == true && isShow == true)
        {
            LoadStudentRecords();
            // Draw the records if they exist
            if (studentRecords.Count > 0 && Showspiceficstudent == false)
            {
                g.DrawString("Name", new Font("Arial", 20, FontStyle.Bold), Brushes.Green, new PointF(200, 60));
                g.DrawString("Average Score", new Font("Arial", 20, FontStyle.Bold), Brushes.Red, new PointF(600, 60));
                g.DrawString("Average Mistake", new Font("Arial", 20, FontStyle.Bold), Brushes.White, new PointF(900, 60));

                float yOffset = 100; // Starting Y position for records
                HashSet<string> displayedNames = new HashSet<string>(); // To track displayed names

                foreach (var record in studentRecords)
                {
                    var p = record.Split(',');
                    string names = p[0].Trim();
                    string points = p[1].Trim();
                    string error = p[2].Trim();

                    if (!displayedNames.Contains(names)) // Check if the name has already been displayed
                    {
                        int avgrecord=0;
                        int avgmistake=0;
                        int ct = 0;
                        foreach( var value in studentRecords)
                        {
                            var pn = value.Split(',');
                            if (names == pn[0].Trim())
                            {
                                avgrecord += int.Parse(pn[1].Trim());
                                avgmistake += int.Parse(pn[2].Trim());
                                ct++;
                            }
                        }

                        g.DrawString(names, new Font("Arial", 14), Brushes.White, new PointF(200, yOffset));
                        g.DrawString((avgrecord/ct).ToString(), new Font("Arial", 14), Brushes.White, new PointF(600, yOffset));
                        g.DrawString((avgmistake/ct).ToString(), new Font("Arial", 14), Brushes.White, new PointF(900, yOffset));

                        displayedNames.Add(names); // Mark the name as displayed
                        yOffset += 25; // Increase Y position for the next record
                    }
                }
            }
            else if(studentRecords.Count < 0 && Showspiceficstudent == false)
            {
                g.DrawString("No records found.", new Font("Arial", 14), Brushes.White, new PointF(450, 320));
            }
            if (objectList.Count > 0)
            {
                lock (objectList)
                {
                    foreach (TuioObject tobj in objectList.Values)
                    {
                        int ox = tobj.getScreenX(width);
                        int oy = tobj.getScreenY(height);
                        int size = height / 4;

                        g.TranslateTransform(ox, oy);
                        g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-ox, -oy);

                        switch (tobj.SymbolID)
                        {
                            case 1:
                                _ = ActivateViewStudentRecords(3);
                                break;
                            case 5:
                                Invalidate();
                                Showspiceficstudent = true;
                                Namespiceficstudent = "MohammedAdnan";
                                break;
                            case 7:
                                Showspiceficstudent = false;
                                Namespiceficstudent = "";
                                break;
                        }
                    }
                }
            }
            if(Showspiceficstudent == true)
            {
                Loadspecificstudent(Namespiceficstudent, g);
            }
        }
        else if (isLogin == true && isRunning == false && showmyresuts == false)
        {
            drawmenu(pevent, g);
            if (objectList.Count > 0)
            {
                lock (objectList)
                {
                    foreach (TuioObject tobj in objectList.Values)
                    {
                        int ox = tobj.getScreenX(width);
                        int oy = tobj.getScreenY(height);
                        int size = height / 4;
                        g.TranslateTransform(ox, oy);
                        g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-ox, -oy);

                        switch (tobj.SymbolID)
                        {
                            case 1:
                                if (tobj.AngleDegrees >= 10 && tobj.AngleDegrees <= 90)
                                {
                                    showmyresuts = false;
                                    startBrush = Brushes.DarkGreen;
                                    _ = ActivateStartMenuOption(1);
                                }
                                else if (tobj.AngleDegrees >= 95 && tobj.AngleDegrees <= 265)
                                {
                                    isRunning = false;
                                    showspecificplayerresults = Brushes.DarkViolet;
                                    _ = ActivateStartMenuOption(3);
                                }
                                else if (tobj.AngleDegrees >= 272 && tobj.AngleDegrees <= 360)
                                {
                                    endBrush = Brushes.DarkRed;
                                    _ = ActivateStartMenuOption(2);
                                }
                                else
                                {
                                    startBrush = Brushes.Green;
                                    endBrush = Brushes.Red;
                                    showspecificplayerresults = Brushes.Indigo;
                                }
                                break;
                        }
                    }
                }
            }
        }
        else if (isTeacherLogin == true && isShow == false)
        {
            g.DrawString("Teacher Menu", new Font("Arial", 25, FontStyle.Bold), Brushes.White, new PointF(480, 30));
            // Draw circle background
            g.FillEllipse(bgrBrush, 400, 100, 400, 400);

            // Define angles for each section
            float startAngle = 270; // Starting angle for the first section
            float sweepAngle = 90;  // Angle for each section

            // Draw sections
            g.FillPie(startBrush, 400, 100, menuSize1, menuSize1, startAngle, sweepAngle); // "Start" section
            startAngle += sweepAngle; // Move to next section

            g.FillPie(startBrush, 400, 100, menuSize2, menuSize2, startAngle, sweepAngle); // "Middle" section
            startAngle += sweepAngle; // Move to next section

            g.FillPie(endBrush, 400, 100, menuSize3, menuSize3, startAngle, sweepAngle); // "End" section
            startAngle += sweepAngle; // Move to next section

            g.FillPie(endBrush, 400, 100, menuSize4, menuSize4, startAngle, sweepAngle); // "Another" section

            // Draw labels for each section
            g.DrawString("viewStudentRecords", new Font("Arial", 18), Brushes.White, new PointF(720, 280));
            g.DrawString("Exit", new Font("Arial", 18), Brushes.White, new PointF(420, 280));

            // Draw inner circle to create an empty effect
            g.FillEllipse(bgrBrush, 500, 200, 200, 200); // Adjust size and position as needed

            if (objectList.Count > 0)
            {
                lock (objectList)
                {
                    foreach (TuioObject tobj in objectList.Values)
                    {
                        int ox = tobj.getScreenX(width);
                        int oy = tobj.getScreenY(height);
                        int size = height / 4;
                        g.TranslateTransform(ox, oy);
                        g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-ox, -oy);

                        switch (tobj.SymbolID)
                        {
                            case 1:
                                if (tobj.AngleDegrees >= 10 && tobj.AngleDegrees <= 175)
                                {
                                    startBrush = Brushes.DarkGreen;
                                    _ = ActivateViewStudentRecords(1);
                                }
                                else if (tobj.AngleDegrees >= 180 && tobj.AngleDegrees <= 340)
                                {
                                    endBrush = Brushes.DarkRed;
                                    _ = ActivateStartMenuOption(2);
                                }
                                else
                                {
                                    startBrush = Brushes.Green;
                                    endBrush = Brushes.Red;
                                }
                                Invalidate();
                                break;
                        }
                    }
                }
            }
        }
        if (isLogin == false && isTeacherLogin == false)
        {
            string text = "Login";
            Font font = new Font("Arial", 24, FontStyle.Bold);
            SizeF textSize = g.MeasureString(text, font);
            float x = (this.ClientSize.Width - textSize.Width) / 2; // Center the text horizontally
            float y = (this.ClientSize.Height - textSize.Height) / 2; // Center the text vertically
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, width, height));
            g.DrawString(text, font, Brushes.White, x, y); // Draw the text in white
        }
        if (isPinch)
        {
            if (isRunning == true)
            {
                WriteScoreToFile("yomama", score, mistakes, cegypt, cgermany, cspain);
            }
            score = 0;
            mistakes = 0;
            isLogin = false;
            isTeacherLogin = false;
            isRunning = false;
            isShow = false;
            isPinch = false;



        }
        if (showmyresuts == true)
        {
            Invalidate();
            LoadMyRecords();
            // Draw the records if they exist
            if (MyRecords.Count > 0)
            {
                g.DrawString("Name", new Font("Arial", 20, FontStyle.Bold), Brushes.Green, new PointF(200, 60));
                g.DrawString("Score", new Font("Arial", 20, FontStyle.Bold), Brushes.Red, new PointF(600, 60));
                g.DrawString("Mistake", new Font("Arial", 20, FontStyle.Bold), Brushes.White, new PointF(900, 60));

                float yOffset = 100; // Starting Y position for records
                foreach (var record in MyRecords)
                {
                    var p = record.Split(',');
                    string names = p[0].Trim();
                    string points = p[1].Trim();
                    string error = p[2].Trim();
                    g.DrawString(names, new Font("Arial", 14), Brushes.White, new PointF(200, yOffset));
                    g.DrawString(points, new Font("Arial", 14), Brushes.White, new PointF(600, yOffset));
                    g.DrawString(error, new Font("Arial", 14), Brushes.White, new PointF(900, yOffset));
                    yOffset += 25; // Increase Y position for the next record
                }
            }
            else
            {
                g.DrawString("No records found.", new Font("Arial", 14), Brushes.White, new PointF(450, 320));
            }
            if (objectList.Count > 0)
            {
                lock (objectList)
                {
                    foreach (TuioObject tobj in objectList.Values)
                    {
                        int ox = tobj.getScreenX(width);
                        int oy = tobj.getScreenY(height);
                        int size = height / 4;
                        bool isCorrect = false; // To track if the object is correctly placed
                        bool isWrong = false;

                        g.TranslateTransform(ox, oy);
                        g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-ox, -oy);
                        switch (tobj.SymbolID)
                        {
                            case 1:

                                _ = ActivateViewStudentRecords(4);
                                break;
                        }
                    }
                }
            }
        }
    }

   public void Login(String MacAdress)
    {
        string studentName = "";
        if (isLogin == false)
        {
            try
            {
                // Open the loginstudent.txt file and read each line
                using (StreamReader reader = new StreamReader("loginstudent.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Trim the line to remove any whitespace
                        studentName = line.Trim();
                        parts = studentName.Split(',');
                        studentName = parts[0].Trim();

                        playername = parts[1].Trim();

                        // Compare the student name with the target name
                        if (studentName.Equals(macmessage, StringComparison.OrdinalIgnoreCase))
                        {

                            isLogin = true;
                            break;
                        }
                    }
                }
                if (playername != "")
                {
                    using (StreamReader reader2 = new StreamReader("student.txt"))
                    {
                        string line2;
                        while ((line2 = reader2.ReadLine()) != null)
                        {

                            string paststudentname = line2.Trim();
                            parts = paststudentname.Split(',');
                            paststudentname = parts[0].Trim();
                            if (playername == paststudentname)
                            {
                                cegypt = int.Parse(parts[3].Trim());
                                cgermany = int.Parse(parts[4].Trim());
                                cspain = int.Parse(parts[5].Trim());
                                score = int.Parse(parts[1].Trim());
                                mistakes = int.Parse(parts[2].Trim());
                            }
                        }
                    }
                }
                else
                {
                    using (StudentNameForm nameForm = new StudentNameForm())
                    {
                        if (nameForm.ShowDialog() == DialogResult.OK && playername == "")
                        {
                            playername = nameForm.StudentName;

                            // Append the new student to the "student.txt" file
                            try
                            {
                                // Read all lines into memory
                                string[] lines = File.ReadAllLines("loginstudent.txt");

                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string[] parts = lines[i].Split(',');
                                    string macInFile = parts[0].Trim();

                                    // Locate the line with the current MAC address
                                    if (macInFile.Equals(studentName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Update the line by appending the playername
                                        lines[i] = $"{macInFile},{playername}";
                                        break;
                                    }
                                }

                                // Write all lines back to the file
                                File.WriteAllLines("loginstudent.txt", lines);

                                MessageBox.Show("Student name updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to update file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            // Update the current session's variables
                            cegypt = 0;
                            cgermany = 0;
                            cspain = 0;
                            score = 0;
                            mistakes = 0;

                            Invalidate(); // Refresh the main form
                        }
                    }
                }
                Invalidate();
                Console.WriteLine($"{studentName} has successfully logged in.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The file loginstudent.txt was not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }



    }

    public static void Main(String[] argv)
    {
        int port = 0;
        switch (argv.Length)
        {
            case 1:
                port = int.Parse(argv[0], null);
                if (port == 0) goto default;
                break;
            case 0:
                port = 3333;
                break;
            default:
                Console.WriteLine("usage: mono TuioDemo [port]");
                System.Environment.Exit(0);
                break;
        }

        TuioDemo app = new TuioDemo(port);
        Application.Run(app);
    }

    public void SendCountryName(TuioObject markerData)
    {

        try
        {

            string countryName = "keep trying";
            // Replace with your TUIO marker data
            switch (markerData.SymbolID)
            {
                case 4:  // Germany case
                    countryName = $"Germany is in the correct spot!! {markerData.AngleDegrees}";
                    break;
                case 3:  // Spain case
                    countryName = $"Spain is in the correct spot!! {markerData.AngleDegrees}";
                    break;
                case 0:  // Egypt case
                    countryName = $"Egypt is in the correct spot!! {markerData.AngleDegrees}";
                    break;
                default:
                    break;
            }


            // Convert the marker data to byte array
            byte[] data = Encoding.UTF8.GetBytes(countryName);


            // Send the marker data to the server
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Sent: {0}", countryName);


        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
    }

    private void StartConnection()
    {
        string server = "MohammedAdnan"; // Server address
        int port = 8000; // Server port

        try
        {
            client1 = new TcpClient(server, port);

            string message = "Hello, Server!";
            byte[] data = Encoding.UTF8.GetBytes(message);

            // Get the network stream
            stream = client1.GetStream();

            // Send the message to the server
            stream.Write(data, 0, data.Length);
            Debug.WriteLine($"Sent: {message}");

            listenerThread = new Thread(ListenForMessages);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to connect: {ex.Message}");
        }
    }

    public void LoginForTeacher(String MacAdress)
    {
        if (isTeacherLogin == false)
        {
            try
            {
                // Open the loginteacher.txt file and read each line
                using (StreamReader reader = new StreamReader("loginteacher.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Trim the line to remove any whitespace
                        string teacherName = line.Trim();
                        // Compare the teacher name with the target name
                        if (teacherName.Equals(macmessage, StringComparison.OrdinalIgnoreCase))
                        {
                            isTeacherLogin = true;
                            Invalidate(); // Refresh the UI
                            Console.WriteLine($"{teacherName} has successfully logged in as a teacher.");
                            break; // Exit the loop once the teacher is found
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The file loginteacher.txt was not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    public void setpinch(String message)
    {
        string[] parts = message.Split(new char[] { ',' }, 2);
        if (parts[0].Trim().Equals("pinching"))
        {
            isPinch = true;
        }
        else
        {
            isPinch = false;
        }
    }

    // Modify ListenForMessages to handle teacher login
    private void ListenForMessages()
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    macmessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (isTeacherLogin == false)
                    {
                        LoginForTeacher(macmessage); // Handle teacher login
                    }
                    if (isLogin == false && isTeacherLogin == false)
                    {
                        Login(macmessage); // Handle student login
                    }
                    if (isLogin && isPinch == false || isTeacherLogin && isPinch == false)
                    {
                        setpinch(macmessage);
                    }

                    Debug.WriteLine($"Received from server: {macmessage}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connection lost: {ex.Message}");
        }
    }
    private void WriteScoreToFile(string macAddress, int score, int mistakes, int cegypt, int cgermany, int cspain)
    {
        string filePath = "student.txt"; // Define the path for the text file
        string name = playername; // Assuming parts[1].Trim() contains the name

        // Create the new record
        string newRecord = $"{name}, {score}, {mistakes} , {cegypt} ,{cgermany},{cspain}";

        // Append the new record to the file
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true)) // 'true' for appending
            {
                writer.WriteLine(newRecord);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error writing to file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadStudentRecords()
    {
        string filePath = "student.txt"; // Path to the text file

        if (File.Exists(filePath))
        {
            studentRecords.Clear(); // Clear previous records
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        studentRecords.Add(line); // Add each line to the list
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading from file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show("No student records found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void LoadMyRecords()
    {
        string filePath = "student.txt"; // Path to the text file

        if (File.Exists(filePath))
        {
            MyRecords.Clear(); // Clear previous records
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string paststudentname = line.Trim();
                        parts = paststudentname.Split(',');
                        paststudentname = parts[0].Trim();
                        if (playername == paststudentname)
                        {
                            MyRecords.Add(line); // Add each line to the list
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading from file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show("No student records found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void Loadspecificstudent(string name , Graphics g)
    {
        Invalidate();
        float yOffset = 100; // Starting Y position for records

        g.DrawString("Name", new Font("Arial", 20, FontStyle.Bold), Brushes.Green, new PointF(200, 60));
        g.DrawString("Score", new Font("Arial", 20, FontStyle.Bold), Brushes.Red, new PointF(600, 60));
        g.DrawString("Mistake", new Font("Arial", 20, FontStyle.Bold), Brushes.White, new PointF(900, 60));

        foreach (var value in studentRecords)
        {
            var p = value.Split(',');
            string names = p[0].Trim();
            string points = p[1].Trim();
            string error = p[2].Trim();


            g.DrawString(names, new Font("Arial", 14), Brushes.White, new PointF(200, yOffset));
            g.DrawString(points, new Font("Arial", 14), Brushes.White, new PointF(600, yOffset));
            g.DrawString(error, new Font("Arial", 14), Brushes.White, new PointF(900, yOffset));

            yOffset += 25;
        }
    }
}
