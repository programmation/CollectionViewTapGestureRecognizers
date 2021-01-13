namespace CollectionViewTapGestureRecognizers

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms

module TestPage =
    type Msg = 
    | PushMsg of int * int
    | CloseMsg of int

    type ExtMsg =
    | NoOp
    | PushRequested of int
    | CloseRequested of int

    type Model = {
        Id: int
    }

    let init (id: int) = 
        let initialModel = {
            Id = id
        }
        initialModel, Cmd.none

    let update msg model =
        match msg with
        | PushMsg (id, toId) -> model, Cmd.none, ExtMsg.PushRequested toId
        | CloseMsg id -> model, Cmd.none, ExtMsg.CloseRequested id

    let view (model:Model) dispatch =
        View.Grid(
            coldefs = [ Star ],
            rowdefs = [
                Star
                Absolute 36.
                Absolute 36.
            ],
            children = [
                View.Label(
                    text = string model.Id,
                    fontSize = FontSize.fromNamedSize(NamedSize.Large),
                    horizontalOptions = LayoutOptions.Center,
                    verticalOptions = LayoutOptions.Center
                ).Column(0).Row(0)
                View.Button(
                    text = "Push next",
                    command = fun _ -> dispatch (PushMsg (model.Id, model.Id + 1))
                ).Column(0).Row(1)
                View.Button(
                    text = "Close",
                    command = fun _ -> dispatch (CloseMsg model.Id)
                ).Column(0).Row(2)
            ]
        )

