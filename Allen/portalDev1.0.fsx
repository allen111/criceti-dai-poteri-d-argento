open System.Windows.Forms
open System.Drawing
open System.Collections.Generic


let f= new Form(TopMost=true)
f.Show()

    
type Portal() =
    let mutable dead= false
    let mutable position=Point()
    let mutable time= new Timer()
    let timeSet (x:int)=
        time.Interval<-x
    let timestart ()=
        time.Start()
    let timestop ()=
        time.Stop()
    do time.Tick.Add(fun e -> dead<-true;f.Invalidate() )
    let paint (g:Graphics)=
       if not dead then
        g.FillRectangle(Brushes.Blue,position.X,position.Y,20,20)
    member this.Position
        with get()=position
        and set(v)=position<-v
    member this.Paint=paint
    member this.Time
        with get()=time
        and set(v)=time<-v
    member this.Dead
        with get()=dead
    member this.TimeSet=timeSet
    member this.TimeStart=timestart
    member this.TimeStop=timestop
    member this.TimeGet
        with get()=time.Interval


type PortalController() as this=
    let mutable user=Point()
    let MaxDistQuad=50000
    let portals= new List<Portal>()
    let mutable preview=0
    let mutable numDead=0
    let mutable positionPreview=Point()
    let mutable prev=false
    let mutable deads= new List<Portal>()
    let checkPreview (pos:Point)=
        let mutable res=0
        let bound= new Region(Rectangle(user.X,user.Y,20,20))
        let tmpR= new Rectangle(pos.X-10,pos.Y-10,20,20)
        if ((positionPreview.X-user.X)*(positionPreview.X-user.X))+((positionPreview.Y-user.Y)*(positionPreview.Y-user.Y)) < MaxDistQuad then
            if bound.IsVisible(tmpR) then
                res<-2
            else
                res<-1
            
        else
            res<-2
        res

    let startPreview (pos:Point)=
        positionPreview<-Point(pos.X-10,pos.Y-10)
        preview<-checkPreview pos
        prev<-true
    let continuePreview (pos:Point)=
       if prev then
        positionPreview<-Point(pos.X-10,pos.Y-10)
        preview<-checkPreview pos
    let timer (t:int)=
        portals |> Seq.iter (fun b->
            
            b.TimeStop()
            b.TimeSet(t)            
            b.TimeStart()
                
            )
    let endPreview (pos:Point)=
      if preview=1 && portals.Count<=4 then
        let tmpPort=new Portal(Position=Point(pos.X-10,pos.Y-10))
        portals.Add(tmpPort)
        match portals.Count with
            |1->timer(60000)
            |2->timer(30000)
            |3->timer(20000)
            |4->timer(1000)
            |5->timer(60)
            |_->()
        
        
      else
        printfn "not posable"
        
      preview<-0
      prev<-false


    let paint (g:Graphics)=
        deads |> Seq.iter (fun i->
           let a= portals.Remove(i)
           ()   
            )
        deads.Clear()

        if preview=1 then
            g.FillRectangle(Brushes.Green,positionPreview.X,positionPreview.Y,20,20)
        if preview=2 then
            g.FillRectangle(Brushes.Red,positionPreview.X,positionPreview.Y,20,20)
        
        portals |> Seq.iteri (fun i b->
            
            if b.Dead then
                deads.Add(b)
            else
                b.Paint(g)
            )

    member this.User
        with get()=user
        and set(v)=user<-v
    member this.StartPreview=startPreview
    member this.ContinuePreview=continuePreview
    member this.EndPreview=endPreview
    member this.Paint=paint

type ed() as this=
    inherit UserControl()
    let usr=Point(300,300)
    let PC=new PortalController(User=usr)
    let t= new Timer(Interval=100)
    do t.Tick.Add(fun e->(this.Invalidate()))
    do t.Start()

    override this.OnMouseDown e=
        PC.StartPreview e.Location
        this.Invalidate()
    override this.OnMouseMove e=
        PC.ContinuePreview e.Location
        this.Invalidate()
    override this.OnMouseUp e=
        PC.EndPreview e.Location
        this.Invalidate()
    override this.OnPaint e=
        e.Graphics.FillRectangle(Brushes.Black,usr.X,usr.Y,20,20)
        PC.Paint e.Graphics


let s= new ed(Dock=DockStyle.Fill)
f.Controls.Add(s)
