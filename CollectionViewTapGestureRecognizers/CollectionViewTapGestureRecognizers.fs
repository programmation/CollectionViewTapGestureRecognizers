namespace CollectionViewTapGestureRecognizers

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms

module App = 
    type Test = {
        Id: int
        Visited: bool
    }
    
    type Model = { 
        PageModels: TestPage.Model list
        Tests: Test list
    }

    type Msg = 
        | TestPageMsg of TestPage.Msg
        | NavigateToDetailMsg of int
        | CloseDetailMsg of int

    let initModel = {
        PageModels = []
        Tests = [ 0 .. 1000 ] |> List.map(fun t -> { Test.Id = t; Visited = false })
    }

    let init () = 
        initModel, Cmd.none

    let handleTestPageExtMsg extMsg =
        match extMsg with
        | TestPage.NoOp -> Cmd.none
        | TestPage.PushRequested id -> Cmd.ofMsg(NavigateToDetailMsg id)
        | TestPage.CloseRequested id -> Cmd.ofMsg(CloseDetailMsg id)

    let handleTestPageMsg msg model =
        let id = 
            match msg with
            | TestPage.PushMsg (id, _) -> id
            | TestPage.CloseMsg id -> id

        match model.PageModels, id with
        | [], _ -> 
            model, Cmd.none
        | pageModels, id -> 
            let pageModel =
                pageModels
                |> List.find(fun p -> p.Id = id)
            let m, cmd, extMsg = TestPage.update msg pageModel
            let cmd2 = handleTestPageExtMsg extMsg
            let batchCmd = Cmd.batch [
                Cmd.map TestPageMsg cmd
                cmd2
            ]
            let pageModels2 =
                pageModels
                |> List.map(fun p -> if p.Id = id then m else p)
            { model with PageModels = pageModels2 }, batchCmd

    let navigateToDetail model id =
        let pageModel, pageCmd = TestPage.init(id)
        let pageModels =
            [ (pageModel) ]
            |> List.append(model.PageModels)
        let tests = 
            model.Tests
            |> List.map(fun t -> if t.Id = id then { t with Visited = true } else t)
        { model with PageModels = pageModels; Tests = tests }, Cmd.map TestPageMsg pageCmd

    let closeDetail model id =
        let pageModels =
            model.PageModels
            |> List.filter(fun p -> p.Id <> id)
        { model with PageModels = pageModels }, Cmd.none

    let update msg model =
        match msg with
        | TestPageMsg msg -> handleTestPageMsg msg model
        | NavigateToDetailMsg id -> navigateToDetail model id
        | CloseDetailMsg id -> closeDetail model id

    let view (model: Model) dispatch =
        let template t =
            View.StackLayout(
                children = [
                    View.Label(
                        text = sprintf "Test %d" t.Id,
                        padding = Thickness(16.),
                        fontSize = FontSize.fromNamedSize(NamedSize.Small),
                        fontAttributes = if t.Visited then FontAttributes.Bold else FontAttributes.None
                    )
                ],
                gestureRecognizers = [
                    View.TapGestureRecognizer(command = fun _ -> dispatch (NavigateToDetailMsg t.Id))
                ]
            )

        let testTemplates =
            model.Tests
            |> List.map(fun t -> template t)

        let homePage = 
            View.ContentPage(
                content = 
                    View.CollectionView(
                        items = testTemplates,
                        selectionMode = SelectionMode.None
                    )
            )

        let testPages =
            model.PageModels
            |> List.map(fun p -> TestPage.view p (TestPageMsg >> dispatch))
            |> List.map(fun p -> View.ContentPage(content = p))

        View.NavigationPage(
            pages = [
                yield homePage
                for testPage in testPages do yield testPage.HasBackButton(false)
            ]
        )

    // Note, this declaration is needed if you enable LiveUpdate
    let program =
        XamarinFormsProgram.mkProgram init update view
#if DEBUG
        |> Program.withConsoleTrace
#endif

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


