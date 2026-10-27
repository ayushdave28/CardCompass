namespace CardCompass.Web.Models.Entities
{
    public enum CardNetwork
    {
        Visa,
        Mastercard,
        RuPay,
        AmericanExpress,
        DinersClub
    }

    public enum CardCategory
    {
        Cashback,
        Rewards,
        Travel,
        Shopping,
        Fuel,
        Dining,
        Lifestyle,
        UPI,
        Business
    }

    public enum RewardType
    {
        Cashback,
        RewardPoints,
        AirMiles,
        NeuCoins,
        EdgeMiles
    }

    public enum SpendingCategory
    {
        GeneralSpends,
        OnlineShopping,
        Grocery,
        Dining,
        Travel,
        Fuel,
        Utilities,
        Entertainment,
        UPI,
        International
    }

    public enum EmploymentType
    {
        Any,
        Salaried,
        SelfEmployed,
        Student
    }

    public enum BenefitType
    {
        LoungeAccess,
        MilestoneBonus,
        DiningDiscount,
        FuelSurchargeWaiver,
        Insurance,
        GolfAccess,
        MovieTicket,
        WelcomeVoucher,
        Other
    }
}
