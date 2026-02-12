using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Runtime.CompilerServices;

public class ProgressoCad
{
    private static ProgressoCad _prog { get; set; }
    private ProgressMeter _pm { get; set; }

    private int _lastPercent { get; set; } = 0;
    private bool _active { get; set; } = false;
    private int _max { get; set; } = 0;
    private int _value { get; set; } = 0;
    private string _msg { get; set; } = "";
    private ProgressoCad(int max, string msg)
    {
        this._max = max;
        this._msg = msg;
    }
    public static ProgressoCad Start(int max, string msg)
    {
        if (_prog != null)
        {
            _prog.Close();
        }

        _prog = new ProgressoCad(max, msg);

        _prog.Update(msg, 0);
        return _prog;
    }
    public void somaProgresso(string msg = null)
    {
        if (msg != null)
        {
            _msg = msg;
        }
        _value += 1;
        Update(_msg, _value);
    }
    private void Update(string text, int actual)
    {

        // Inicia o ProgressMeter
        if (!_active)
        {
            _pm = new ProgressMeter();
            _pm.SetLimit(_max);
            _pm.Start(text);
            _active = true;
        }

        // Atualiza texto no status bar
        var ed = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage($"\n({actual}/{_max}) - {text}");

        // Avança somente se aumentou
        while (_lastPercent < actual)
        {
            _pm.MeterProgress();
            _lastPercent++;
        }

        // Se chegou a 100%, finaliza automaticamente
        if (actual >= 100)
            this.Close();
    }

    public void Close()
    {
        if (_active)
        {
            _pm.Stop();
            _active = false;
            _lastPercent = 0;

            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\nProcesso concluído.");
        }
    }
}
