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


public class TuioDemo : Form, TuioListener
{
	private TuioClient client;
	private Dictionary<long, TuioObject> objectList;
	private Dictionary<long, TuioCursor> cursorList;
	private Dictionary<long, TuioBlob> blobList;


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
    public string serverIP = "DESKTOP-1RLK4BP";

    private bool isRunning = false; // Flag to manage application state
    private int menuSize1 = 400;
    private int menuSize2 = 400;

    private bool fullscreen;
	private bool verbose;

	Font font = new Font("Arial", 10.0f);
	SolidBrush fntBrush = new SolidBrush(Color.White);
	SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
	SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
	SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
	SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
	Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);
	private string objectImagePath;
	private string backgroundImagePath;
	private string markimagepath;
	private string crossimagepath;
	TcpClient client1;
	NetworkStream stream;

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

    private async Task ActivateStartMenuOption( int flag)
    {
        await Task.Delay(2000);
        if(flag==1)
        {
            isRunning = true;
        }
        else
        {
            stream.Close();
            client1.Close();
            this.Close();
        }
    }
    protected override void OnPaintBackground(PaintEventArgs pevent)

	{
		// Getting the graphics object
		Graphics g = pevent.Graphics;
		g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));

        if (isRunning)
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
                            case 2:
                                isRunning = false;
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
                                g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
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
        else
        {
            // Draw circle background
            g.FillEllipse(bgrBrush, 400, 100, 400, 400);

            // Draw Start and End sectors
            g.FillPie(Brushes.Green, 400, 100, menuSize1, menuSize1, 270, 180); // "Start" half
            g.FillPie(Brushes.Red, 400, 100, menuSize2, menuSize2, 270, -180); // "End" half

            // Draw labels for Start and End options
            g.DrawString("Start", new Font("Arial", 18), Brushes.White, new PointF(650, 300));
            g.DrawString("End", new Font("Arial", 18), Brushes.White, new PointF(450, 300));

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
                                if (tobj.AngleDegrees >= 20 && tobj.AngleDegrees <= 80)
                                {
                                    menuSize1 = 430;
                                    menuSize2 = 400;
                                    _ = ActivateStartMenuOption(1);
                                }
                                else if(tobj.AngleDegrees >= 300 && tobj.AngleDegrees <= 340)
                                {
                                    menuSize2 = 430;
                                    menuSize1 = 400;
                                    _ = ActivateStartMenuOption(2);

                                }
                                else
                                {
                                    menuSize1 = 400;
                                    menuSize2 = 400;
                                }
                                break;
                        }
                    }
                }
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
        string server = "DESKTOP-1RLK4BP"; // Server address
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
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					Debug.WriteLine($"Received from server: {message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connection lost: {ex.Message}");
        }
    }

}
