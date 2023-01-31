using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArisDev
{
    public class Utility
    {
        public string GetNseErrorMessage(string exchange, short errorCode)
        {
            string message = errorCode.ToString() + " ";

            if (exchange == "NseCm")
            {
                switch (errorCode)
                {
                    case 293:
                        message += "Invalid instrument type.";
                        break;

                    case 509:
                        message += "Order does not exit.";
                        break;

                    case 8049:
                        message += "Initiator is not allowed to cancel auction order.";
                        break;

                    case 8485:
                        message += "Auction number does not exit.";
                        break;

                    case 8661:
                        message += "Delivery start number must less than or equal to Delivery end number.";
                        break;

                    case 16000:
                        message += "The trading system is not available for trading.";
                        break;

                    case 16003:
                        message += "System error was encountered. Please call the Exchange.";
                        break;

                    case 16004:
                        message += "The user is already signed on.";
                        break;

                    case 16005:
                        message += "System error while trying to sign-off. Please call the Exchange.";
                        break;

                    case 16006:
                        message += "Invalid signon Please try again.";
                        break;

                    case 16011:
                        message += "This report has already been requested.";
                        break;

                    case 16012:
                        message += "Invalid contract descriptor.";
                        break;

                    case 16014:
                        message += "This order is not yours.";
                        break;

                    case 16015:
                        message += "This trade is not yours.";
                        break;

                    case 16016:
                        message += "Invalid trade number.";
                        break;

                    case 16019:
                        message += "Stock not found.";
                        break;

                    case 16035:
                        message += "Contract is unavailable for trading at this time. Please try later.";
                        break;

                    case 16041:
                        message += "Trading member does not exit in the system.";
                        break;

                    case 16042:
                        message += "Dealer does not exist in the system.";
                        break;

                    case 16043:
                        message += "This record already exits on the NEAT system.";
                        break;

                    case 16044:
                        message += "Order has been modified. Please try again.";
                        break;

                    case 16049:
                        message += "Stock is SUSPENDED.";
                        break;

                    case 16053:
                        message += "Your password has expired, must be changed.";
                        break;

                    case 16054:
                        message += "Invalid branch for trading member.";
                        break;

                    case 16056:
                        message += "Program error.";
                        break;

                    case 16086:
                        message += "Duplicate trade cancel request.";
                        break;

                    case 16098:
                    case 16099:
                        message += "Invalid trader id for buyer.";
                        break;

                    case 16104:
                        message += "System could not complete your transaction - ADMIN notified.";
                        break;

                    case 16148:
                        message += "Invalid Dealer Id entered.";
                        break;

                    case 16154:
                        message += "Invalid Trader id entered.";
                        break;

                    case 16169:
                        message += "Order priced ATO cannot be entered when a security is open.";
                        break;

                    case 16172:
                        message += "Prices freeze are not allowed.";
                        break;

                    case 16179:
                        message += "Order price is out of daily price range.";
                        break;

                    case 16198:
                        message += "Duplicate modification or cancellation request for the same trade has been encountered.";
                        break;

                    case 16159:
                        message += "No market orders are allowed in PostClose.";
                        break;

                    case 16668:
                        message += "Orders not allowed in PostClose.";
                        break;

                    case 16228:
                        message += "SL, MIT or NT Orders are Not Allowed during Post Close.";
                        break;

                    case 16229:
                        message += "GTC GTD or GTDays Orders are Not Allowed during Post Close.";
                        break;

                    case 16230:
                        message += "Continuous session orders cannot be modified.";
                        break;

                    case 16231:
                        message += "Continuous session trades cannot be changed.";
                        break;

                    case 16233:
                        message += "Proprietary requests cannot be made for participant.";
                        break;

                    case 16251:
                        message += "Trade modification with different quantities is received.";
                        break;

                    case 16273:
                        message += "Record does not exit.";
                        break;

                    case 16278:
                        message += "The markets have not been opened for trading.";
                        break;

                    case 16279:
                        message += "The contract has not yet been admitted for trading.";
                        break;

                    case 16280:
                        message += "The Contract has matured.";
                        break;

                    case 16281:
                        message += "The security has been expelled.";
                        break;

                    case 16282:
                        message += "The order quantity is greater than the issued capital.";
                        break;

                    case 16283:
                        message += "The order price is not multiple of the tick size.";
                        break;

                    case 16284:
                        message += "The order price is out of the day’s price range.";
                        break;

                    case 16285:
                        message += "The broker is not active.";
                        break;

                    case 16300:
                        message += "System is in a wrong state to make the requested change.";
                        break;

                    case 16303:
                        message += "The auction is pending.";
                        break;

                    case 16307:
                        message += "The order has been canceled due to quantity freeze.";
                        break;

                    case 16308:
                        message += "The order has been canceled due to price freeze.";
                        break;

                    case 16311:
                        message += "The Solicitor period for the Auction is over.";
                        break;

                    case 16312:
                        message += "The Competitor period for the Auction is over.";
                        break;

                    case 16313:
                        message += "The Auction period will cross market close time.";
                        break;

                    case 16315:
                        message += "The limit price s worse than the trigger price.";
                        break;

                    case 16316:
                        message += "The trigger price is not a multiple of tick size.";
                        break;

                    case 16317:
                        message += "AON attribute not allowed.";
                        break;

                    case 16318:
                        message += "MF attribute not allowed.";
                        break;

                    case 16319:
                        message += "AON attribute not allowed at Security level.";
                        break;

                    case 16320:
                        message += "MF attribute not allowed at security level.";
                        break;

                    case 16321:
                        message += "MF quantity is greater than disclosed quantity";
                        break;

                    case 16322:
                        message += "MF quantity is not a multiple of regular lot.";
                        break;

                    case 16323:
                        message += "MF quantity is greater than Original quantity.";
                        break;

                    case 16324:
                        message += "Disclosed quantity is greater than Original quantity.";
                        break;

                    case 16325:
                        message += "Disclosed quantity is not a multiple of regular lot.";
                        break;

                    case 16326:
                        message += "GTD is greater than that specified at System.";
                        break;

                    case 16327:
                        message += "Odd lot quantity cannot be greater than or equal to regular lot size.";
                        break;

                    case 16328:
                        message += "Quantity is not a multiple of regular lot.";
                        break;

                    case 16329:
                        message += "Trading Member not permitted in the market.";
                        break;

                    case 16330:
                        message += "Security is suspended.";
                        break;

                    case 16333:
                        message += "Branch Order Value Limit has been exceeded.";
                        break;

                    case 16343:
                        message += "The order to be cancelled has changed.";
                        break;

                    case 16344:
                        message += "The order cannot be cancelled.";
                        break;

                    case 16345:
                        message += "Initiator order cannot be cancelled.";
                        break;

                    case 16346:
                        message += "Order cannot be modified.";
                        break;

                    case 16347:
                        message += "Request rejected as end of day processing has ended.";
                        break;

                    case 16348:
                        message += "Trading is not allowed in this market.";
                        break;

                    case 16357:
                        message += "Control has rejected the Negotiated Trade.";
                        break;

                    case 16363:
                        message += "Status is in the required state.";
                        break;

                    case 16369:
                        message += "Contract is in preopen.";
                        break;

                    case 16372:
                        message += "Order entry not allowed for user as it is of inquiry type.";
                        break;

                    case 16375:
                        message += "Request rejected as end of day processing has started.";
                        break;

                    case 16387:
                        message += "Contract not allowed to trader in.";
                        break;

                    case 16392:
                        message += "Turnover limit not provided. Please contact Exchange.";
                        break;

                    case 16400:
                        message += "DQ is less than minimum allowed.";
                        break;

                    case 16404:
                        message += "Order has been cancelled due to freeze admin suspension.";
                        break;

                    case 16405:
                        message += "BUY – SELL type entered is invalid.";
                        break;

                    case 16415:
                        message += "invalid combination of book type and instructions(order_type).";
                        break;

                    case 16416:
                        message += "invalid combination of mf/aon/disclosed volume.";
                        break;

                    case 16414:
                        message += "Invalid Pro-Client, your user id is not PRO enabled";
                        break;

                    case 16437:
                        message += "Warehouse deleted.";
                        break;

                    case 16440:
                        message += "GTD > Maturity Date.";
                        break;

                    case 16441:
                        message += "DQ orders are not allowed in preopen.";
                        break;

                    case 16442:
                        message += "Order limit exceeds the order value limit.";
                        break;

                    case 16445:
                        message += "Stoploss orders are not allowed.";
                        break;

                    case 16446:
                        message += "Market If Touched orders are not allowed.";
                        break;

                    case 16447:
                        message += "Order entry not allowed in Pre-Open.";
                        break;

                    case 16500:
                        message += "Ex/Pl not allowed.";
                        break;

                    case 16501:
                        message += "Invalid ExPl flag value.";
                        break;

                    case 16513:
                        message += "Ex/Pl rejection not allowed.";
                        break;

                    case 16514:
                        message += "Not modifiable.";
                        break;

                    case 16518:
                        message += "Clearing member , Trading Member link not found.";
                        break;

                    case 16521:
                        message += "Not a clearing member.";
                        break;

                    case 16523:
                        message += "User in not a corporate manager.";
                        break;

                    case 16531:
                        message += "Enter either TM or participant.";
                        break;

                    case 16532:
                        message += "Clearing member Participant link not found.";
                        break;

                    case 16533:
                        message += "enter either TM or Participant.";
                        break;

                    case 16534:
                        message += "Participant/CM id link not found.";
                        break;

                    case 16542:
                        message += "Invalid participant.";
                        break;

                    case 16550:
                        message += "Trade cannot be modified/cancelled - has already been approved by CM.";
                        break;

                    case 16552:
                        message += "Stock has been suspended.";
                        break;

                    case 16554:
                        message += "Trading Member not permitted in Futures.";
                        break;

                    case 16555:
                        message += "Trading Member not permitted in Options.";
                        break;

                    case 16556:
                        message += "Quantity less than the minimum lot size.";
                        break;

                    case 16557:
                        message += "Disclose quantity less than the minimum lot size.";
                        break;

                    case 16558:
                        message += "Minimum fill less than the minimum lot size.";
                        break;

                    case 16560:
                        message += "The giveup trade has already been rejected.";
                        break;

                    case 16561:
                        message += "Negotiated Orders not allowed.";
                        break;

                    case 16562:
                        message += "Negotiated Trade not allowed.";
                        break;

                    case 16566:
                        message += "User does not belong to Broker or Branch.";
                        break;

                    case 16570:
                        message += "The market is in postclose.";
                        break;

                    case 16571:
                        message += "The Closing Session has ended.";
                        break;

                    case 16572:
                        message += "Closing Session trades have been generated.";
                        break;

                    case 16573:
                        message += "Message Length Is Invalid.";
                        break;

                    case 16574:
                        message += "OPEN - CLOSE type entered is invalid.";
                        break;

                    case 16576:
                        message += "No. of nnf inquiry requests exceeded.";
                        break;

                    case 16578:
                        message += "COVER - UNCOVER type entered is invalid.";
                        break;

                    case 16579:
                        message += "Giveup requested for wrong CM Id.";
                        break;

                    case 16580:
                        message += "Order does not belong to the given participant.";
                        break;

                    case 16581:
                        message += "Invalid trade price.";
                        break;

                    case 16583:
                        message += "For Pro Order Participant Entry Not Allowed.";
                        break;

                    case 16585:
                        message += "Not a valid account number.";
                        break;

                    case 16586:
                        message += "Participant Order Entry Not Allowed.";
                        break;

                    case 16589:
                        message += "All continuous session orders are being deleted now.";
                        break;

                    case 16594:
                    case 16596:
                        message += "Trading member cannot put Ex/Pl request for Participant.";
                        break;

                    case 16597:
                        message += "Branch limit should be greater than sum of user limits.";
                        break;

                    case 16598:
                        message += "Branch limit should be greater than used limit.";
                        break;

                    case 16602:
                        message += "dealer_value_limit exceeds set limit.";
                        break;

                    case 16604:
                        message += "Participant not found.";
                        break;

                    case 16605:
                        message += "One leg of spread/2L failed.";
                        break;

                    case 16606:
                        message += "Quantity greater than Freeze quantity.";
                        break;

                    case 16607:
                        message += "Spread Not Allowed.";
                        break;

                    case 16608:
                        message += "Spread Allowed Only When Mkt Is Open.";
                        break;

                    case 16609:
                        message += "Spread Allowed Only When Stock Is Open.";
                        break;

                    case 16610:
                        message += "both legs should have same quantity.";
                        break;

                    case 16611:
                        message += "Modified order qty freeze not allowed.";
                        break;

                    case 16612:
                        message += "The trade record has been modified.";
                        break;

                    case 16615:
                        message += "Order cannot be modified.";
                        break;

                    case 16616:
                        message += "Order can not be cancelled.";
                        break;

                    case 16617:
                        message += "Trade can not be manipulated.";
                        break;

                    case 16619:
                        message += "Pcm can not ex_pl for himself.";
                        break;

                    case 16620:
                        message += "Ex/Pl by clearing member for Tm not allowed.";
                        break;

                    case 16621:
                        message += "Clearing member cannot change the Ex/Pl requests placed by Trading Member.";
                        break;

                    case 16625:
                        message += "Clearing member is suspended.";
                        break;

                    case 16626:
                        message += "expiry date not in ascending order.";
                        break;

                    case 16627:
                        message += "Invalid contract combination";
                        break;

                    case 16628:
                    case 16629:
                    case 16630:
                        message += "";
                        break;

                    case 16631:
                        message += "spread not allowed for different underlying.";
                        break;

                    case 16632:
                        message += "Cli A/c number cannot be modified as Trading member id.";
                        break;

                    case 16636:
                        message += "Futures buy branch Order Value Limit has been exceeded.";
                        break;

                    case 16637:
                        message += "Futures sell branch Order Value Limit has been exceeded.";
                        break;

                    case 16638:
                        message += "Options buy branch Order Value Limit has been exceeded.";
                        break;

                    case 16639:
                        message += "Options sell branch Order Value Limit has been exceeded.";
                        break;

                    case 16640:
                        message += "Futures buy used limit execeeded the user limit.";
                        break;

                    case 16641:
                        message += "Futures sell used limit exceeded the user limit.";
                        break;

                    case 16642:
                        message += "Options buy used limit exceeded the user limit.";
                        break;

                    case 16643:
                        message += "Options sell used limit exceeded the user limit.";
                        break;

                    case 16645:
                        message += "Cannot approve.Bhav Copy generated.";
                        break;

                    case 16646:
                        message += "$p cannot modify";
                        break;

                    case 16664:
                        message += "Quantity is not a multiple of delivery lot.";
                        break;

                    case 16666:
                        message += "Not a valid date for Delivery Request.";
                        break;

                    case 16672:
                        message += "Both TM id and client code required.";
                        break;

                    case 16541:
                        message += "Participant is invalid.";
                        break;

                    case 16577:
                        message += "Both participant and volume changed.";
                        break;

                    case 16692:
                        message += "Nnf field value is <1 or > 999999999999999.";
                        break;

                    case 16684:
                        message += "Password is one of last 5 passwords.";
                        break;

                    case 16683:
                        message += "User password is locked.";
                        break;

                    case 16685:
                        message += "User does not linked to you.";
                        break;

                    case 16695:
                        message += "Only Corporate Manager can change trade modification rights.";
                        break;

                    case 16697:
                        message += "CM/Admin/Inquiry user not allowed.";
                        break;

                    case 16698:
                        message += "Trade modification restricted by Corporate Manager.";
                        break;

                    case 16699:
                        message += "Trigger Price is out of the daily price range.";
                        break;
                }
            }
            else
            {
                switch (errorCode)
                {
                    case 293:
                        message += "Invalid instrument type.";
                        break;

                    case 509:
                        message += "Order does not exit.";
                        break;

                    case 8049:
                        message += "Initiator is not allowed to cancel auction order.";
                        break;

                    case 8485:
                        message += "Auction number does not exit.";
                        break;

                    case 8661:
                        message += "Delivery start number must less than or equal to Delivery end number.";
                        break;

                    case 16000:
                        message += "The trading system is not available for trading.";
                        break;

                    case 16003:
                        message += "System error was encountered. Please call the Exchange.";
                        break;

                    case 16004:
                        message += "The user is already signed on.";
                        break;

                    case 16005:
                        message += "System error while trying to sign-off. Please call the Exchange.";
                        break;

                    case 16006:
                        message += "Invalid signon Please try again.";
                        break;

                    case 16011:
                        message += "This report has already been requested.";
                        break;

                    case 16012:
                        message += "Invalid contract descriptor.";
                        break;

                    case 16014:
                        message += "This order is not yours.";
                        break;

                    case 16015:
                        message += "This trade is not yours.";
                        break;

                    case 16016:
                        message += "Invalid trade number.";
                        break;

                    case 16019:
                        message += "Stock not found.";
                        break;

                    case 16035:
                        message += "Contract is unavailable for trading at this time. Please try later.";
                        break;

                    case 16041:
                        message += "Trading member does not exit in the system.";
                        break;

                    case 16042:
                        message += "Dealer does not exist in the system.";
                        break;

                    case 16043:
                        message += "This record already exits on the NEAT system.";
                        break;

                    case 16044:
                        message += "Order has been modified. Please try again.";
                        break;

                    case 16049:
                        message += "Stock is SUSPENDED.";
                        break;

                    case 16053:
                        message += "Your password has expired, must be changed.";
                        break;

                    case 16054:
                        message += "Invalid branch for trading member.";
                        break;

                    case 16056:
                        message += "Program error.";
                        break;

                    case 16086:
                        message += "Duplicate trade cancel request.";
                        break;

                    case 16098:
                    case 16099:
                        message += "Invalid trader id for buyer.";
                        break;

                    case 16104:
                        message += "System could not complete your transaction - ADMIN notified.";
                        break;

                    case 16148:
                        message += "Invalid Dealer Id entered.";
                        break;

                    case 16154:
                        message += "Invalid Trader id entered.";
                        break;

                    case 16169:
                        message += "Order priced ATO cannot be entered when a security is open.";
                        break;

                    case 16172:
                        message += "Prices freeze are not allowed.";
                        break;

                    case 16179:
                        message += "Order price is out of daily price range.";
                        break;

                    case 16198:
                        message += "Duplicate modification or cancellation request for the same trade has been encountered.";
                        break;

                    case 16159:
                        message += "No market orders are allowed in PostClose.";
                        break;

                    case 16668:
                        message += "Orders not allowed in PostClose.";
                        break;

                    case 16228:
                        message += "SL, MIT or NT Orders are Not Allowed during Post Close.";
                        break;

                    case 16229:
                        message += "GTC GTD or GTDays Orders are Not Allowed during Post Close.";
                        break;

                    case 16230:
                        message += "Continuous session orders cannot be modified.";
                        break;

                    case 16231:
                        message += "Continuous session trades cannot be changed.";
                        break;

                    case 16233:
                        message += "Proprietary requests cannot be made for participant.";
                        break;

                    case 16251:
                        message += "Trade modification with different quantities is received.";
                        break;

                    case 16273:
                        message += "Record does not exit.";
                        break;

                    case 16278:
                        message += "The markets have not been opened for trading.";
                        break;

                    case 16279:
                        message += "The contract has not yet been admitted for trading.";
                        break;

                    case 16280:
                        message += "The Contract has matured.";
                        break;

                    case 16281:
                        message += "The security has been expelled.";
                        break;

                    case 16282:
                        message += "The order quantity is greater than the issued capital.";
                        break;

                    case 16283:
                        message += "The order price is not multiple of the tick size.";
                        break;

                    case 16284:
                        message += "The order price is out of the day’s price range.";
                        break;

                    case 16285:
                        message += "The broker is not active.";
                        break;

                    case 16300:
                        message += "System is in a wrong state to make the requested change.";
                        break;

                    case 16303:
                        message += "The auction is pending.";
                        break;

                    case 16307:
                        message += "The order has been canceled due to quantity freeze.";
                        break;

                    case 16308:
                        message += "The order has been canceled due to price freeze.";
                        break;

                    case 16311:
                        message += "The Solicitor period for the Auction is over.";
                        break;

                    case 16312:
                        message += "The Competitor period for the Auction is over.";
                        break;

                    case 16313:
                        message += "The Auction period will cross market close time.";
                        break;

                    case 16315:
                        message += "The limit price s worse than the trigger price.";
                        break;

                    case 16316:
                        message += "The trigger price is not a multiple of tick size.";
                        break;

                    case 16317:
                        message += "AON attribute not allowed.";
                        break;

                    case 16318:
                        message += "MF attribute not allowed.";
                        break;

                    case 16319:
                        message += "AON attribute not allowed at Security level.";
                        break;

                    case 16320:
                        message += "MF attribute not allowed at security level.";
                        break;

                    case 16321:
                        message += "MF quantity is greater than disclosed quantity";
                        break;

                    case 16322:
                        message += "MF quantity is not a multiple of regular lot.";
                        break;

                    case 16323:
                        message += "MF quantity is greater than Original quantity.";
                        break;

                    case 16324:
                        message += "Disclosed quantity is greater than Original quantity.";
                        break;

                    case 16325:
                        message += "Disclosed quantity is not a multiple of regular lot.";
                        break;

                    case 16326:
                        message += "GTD is greater than that specified at System.";
                        break;

                    case 16327:
                        message += "Odd lot quantity cannot be greater than or equal to regular lot size.";
                        break;

                    case 16328:
                        message += "Quantity is not a multiple of regular lot.";
                        break;

                    case 16329:
                        message += "Trading Member not permitted in the market.";
                        break;

                    case 16330:
                        message += "Security is suspended.";
                        break;

                    case 16333:
                        message += "Branch Order Value Limit has been exceeded.";
                        break;

                    case 16343:
                        message += "The order to be cancelled has changed.";
                        break;

                    case 16344:
                        message += "The order cannot be cancelled.";
                        break;

                    case 16345:
                        message += "Initiator order cannot be cancelled.";
                        break;

                    case 16346:
                        message += "Order cannot be modified.";
                        break;

                    case 16347:
                        message += "Request rejected as end of day processing has ended.";
                        break;

                    case 16348:
                        message += "Trading is not allowed in this market.";
                        break;

                    case 16357:
                        message += "Control has rejected the Negotiated Trade.";
                        break;

                    case 16363:
                        message += "Status is in the required state.";
                        break;

                    case 16369:
                        message += "Contract is in preopen.";
                        break;

                    case 16372:
                        message += "Order entry not allowed for user as it is of inquiry type.";
                        break;

                    case 16375:
                        message += "Request rejected as end of day processing has started.";
                        break;

                    case 16387:
                        message += "Contract not allowed to trader in.";
                        break;

                    case 16392:
                        message += "Turnover limit not provided. Please contact Exchange.";
                        break;

                    case 16400:
                        message += "DQ is less than minimum allowed.";
                        break;

                    case 16404:
                        message += "Order has been cancelled due to freeze admin suspension.";
                        break;

                    case 16405:
                        message += "BUY – SELL type entered is invalid.";
                        break;

                    case 16415:
                        message += "invalid combination of book type and instructions(order_type).";
                        break;

                    case 16416:
                        message += "invalid combination of mf/aon/disclosed volume.";
                        break;

                    case 16414:
                        message += "Invalid Pro-Client, your user id is not PRO enabled";
                        break;

                    case 16437:
                        message += "Warehouse deleted.";
                        break;

                    case 16440:
                        message += "GTD > Maturity Date.";
                        break;

                    case 16441:
                        message += "DQ orders are not allowed in preopen.";
                        break;

                    case 16442:
                        message += "Order limit exceeds the order value limit.";
                        break;

                    case 16445:
                        message += "Stoploss orders are not allowed.";
                        break;

                    case 16446:
                        message += "Market If Touched orders are not allowed.";
                        break;

                    case 16447:
                        message += "Order entry not allowed in Pre-Open.";
                        break;

                    case 16500:
                        message += "Ex/Pl not allowed.";
                        break;

                    case 16501:
                        message += "Invalid ExPl flag value.";
                        break;

                    case 16513:
                        message += "Ex/Pl rejection not allowed.";
                        break;

                    case 16514:
                        message += "Not modifiable.";
                        break;

                    case 16518:
                        message += "Clearing member , Trading Member link not found.";
                        break;

                    case 16521:
                        message += "Not a clearing member.";
                        break;

                    case 16523:
                        message += "User in not a corporate manager.";
                        break;

                    case 16531:
                        message += "Enter either TM or participant.";
                        break;

                    case 16532:
                        message += "Clearing member Participant link not found.";
                        break;

                    case 16533:
                        message += "enter either TM or Participant.";
                        break;

                    case 16534:
                        message += "Participant/CM id link not found.";
                        break;

                    case 16542:
                        message += "Invalid participant.";
                        break;

                    case 16550:
                        message += "Trade cannot be modified/cancelled - has already been approved by CM.";
                        break;

                    case 16552:
                        message += "Stock has been suspended.";
                        break;

                    case 16554:
                        message += "Trading Member not permitted in Futures.";
                        break;

                    case 16555:
                        message += "Trading Member not permitted in Options.";
                        break;

                    case 16556:
                        message += "Quantity less than the minimum lot size.";
                        break;

                    case 16557:
                        message += "Disclose quantity less than the minimum lot size.";
                        break;

                    case 16558:
                        message += "Minimum fill less than the minimum lot size.";
                        break;

                    case 16560:
                        message += "The giveup trade has already been rejected.";
                        break;

                    case 16561:
                        message += "Negotiated Orders not allowed.";
                        break;

                    case 16562:
                        message += "Negotiated Trade not allowed.";
                        break;

                    case 16566:
                        message += "User does not belong to Broker or Branch.";
                        break;

                    case 16570:
                        message += "The market is in postclose.";
                        break;

                    case 16571:
                        message += "The Closing Session has ended.";
                        break;

                    case 16572:
                        message += "Closing Session trades have been generated.";
                        break;

                    case 16573:
                        message += "Message Length Is Invalid.";
                        break;

                    case 16574:
                        message += "OPEN - CLOSE type entered is invalid.";
                        break;

                    case 16576:
                        message += "No. of nnf inquiry requests exceeded.";
                        break;

                    case 16578:
                        message += "COVER - UNCOVER type entered is invalid.";
                        break;

                    case 16579:
                        message += "Giveup requested for wrong CM Id.";
                        break;

                    case 16580:
                        message += "Order does not belong to the given participant.";
                        break;

                    case 16581:
                        message += "Invalid trade price.";
                        break;

                    case 16583:
                        message += "For Pro Order Participant Entry Not Allowed.";
                        break;

                    case 16585:
                        message += "Not a valid account number.";
                        break;

                    case 16586:
                        message += "Participant Order Entry Not Allowed.";
                        break;

                    case 16589:
                        message += "All continuous session orders are being deleted now.";
                        break;

                    case 16594:
                    case 16596:
                        message += "Trading member cannot put Ex/Pl request for Participant.";
                        break;

                    case 16597:
                        message += "Branch limit should be greater than sum of user limits.";
                        break;

                    case 16598:
                        message += "Branch limit should be greater than used limit.";
                        break;

                    case 16602:
                        message += "dealer_value_limit exceeds set limit.";
                        break;

                    case 16604:
                        message += "Participant not found.";
                        break;

                    case 16605:
                        message += "One leg of spread/2L failed.";
                        break;

                    case 16606:
                        message += "Quantity greater than Freeze quantity.";
                        break;

                    case 16607:
                        message += "Spread Not Allowed.";
                        break;

                    case 16608:
                        message += "Spread Allowed Only When Mkt Is Open.";
                        break;

                    case 16609:
                        message += "Spread Allowed Only When Stock Is Open.";
                        break;

                    case 16610:
                        message += "both legs should have same quantity.";
                        break;

                    case 16611:
                        message += "Modified order qty freeze not allowed.";
                        break;

                    case 16612:
                        message += "The trade record has been modified.";
                        break;

                    case 16615:
                        message += "Order cannot be modified.";
                        break;

                    case 16616:
                        message += "Order can not be cancelled.";
                        break;

                    case 16617:
                        message += "Trade can not be manipulated.";
                        break;

                    case 16619:
                        message += "Pcm can not ex_pl for himself.";
                        break;

                    case 16620:
                        message += "Ex/Pl by clearing member for Tm not allowed.";
                        break;

                    case 16621:
                        message += "Clearing member cannot change the Ex/Pl requests placed by Trading Member.";
                        break;

                    case 16625:
                        message += "Clearing member is suspended.";
                        break;

                    case 16626:
                        message += "expiry date not in ascending order.";
                        break;

                    case 16627:
                        message += "Invalid contract combination";
                        break;

                    case 16628:
                    case 16629:
                    case 16630:
                        message += "";
                        break;

                    case 16631:
                        message += "spread not allowed for different underlying.";
                        break;

                    case 16632:
                        message += "Cli A/c number cannot be modified as Trading member id.";
                        break;

                    case 16636:
                        message += "Futures buy branch Order Value Limit has been exceeded.";
                        break;

                    case 16637:
                        message += "Futures sell branch Order Value Limit has been exceeded.";
                        break;

                    case 16638:
                        message += "Options buy branch Order Value Limit has been exceeded.";
                        break;

                    case 16639:
                        message += "Options sell branch Order Value Limit has been exceeded.";
                        break;

                    case 16640:
                        message += "Futures buy used limit execeeded the user limit.";
                        break;

                    case 16641:
                        message += "Futures sell used limit exceeded the user limit.";
                        break;

                    case 16642:
                        message += "Options buy used limit exceeded the user limit.";
                        break;

                    case 16643:
                        message += "Options sell used limit exceeded the user limit.";
                        break;

                    case 16645:
                        message += "Cannot approve.Bhav Copy generated.";
                        break;

                    case 16646:
                        message += "$p cannot modify";
                        break;

                    case 16664:
                        message += "Quantity is not a multiple of delivery lot.";
                        break;

                    case 16666:
                        message += "Not a valid date for Delivery Request.";
                        break;

                    case 16672:
                        message += "Both TM id and client code required.";
                        break;

                    case 16541:
                        message += "Participant is invalid.";
                        break;

                    case 16577:
                        message += "Both participant and volume changed.";
                        break;

                    case 16692:
                        message += "Nnf field value is <1 or > 999999999999999.";
                        break;

                    case 16684:
                        message += "Password is one of last 5 passwords.";
                        break;

                    case 16683:
                        message += "User password is locked.";
                        break;

                    case 16685:
                        message += "User does not linked to you.";
                        break;

                    case 16695:
                        message += "Only Corporate Manager can change trade modification rights.";
                        break;

                    case 16697:
                        message += "CM/Admin/Inquiry user not allowed.";
                        break;

                    case 16698:
                        message += "Trade modification restricted by Corporate Manager.";
                        break;

                    case 16699:
                        message += "Trigger Price is out of the daily price range.";
                        break;
                }
            }
            return message + " ";
        }

        public string GetNseReasonMessage(string exchange, short errorCode)
        {
            if (exchange == "NseCm")
            {
                switch (errorCode)
                {
                    case 2:
                        return "Exercise.";

                    case 3:
                        return "Position liquidation.";

                    case 20:
                    case 5:
                        return "Expl Security.";

                    case 6:
                        return "Broker.";

                    case 7:
                        return "Branch.";

                    case 8:
                        return "User.";

                    case 9:
                        return "Participant.";

                    case 10:
                        return "Counter Party.";

                    case 11:
                        return "Order Number.";

                    case 15:
                        return "Auction Number.";

                    case 16:
                        return "Order.";

                    case 17:
                        return "Price Freeze.";

                    case 18:
                        return "Quantity Freeze.";

                    case 29:
                        return "Invalid Expl.";

                    case 30:
                        return "Exercise Mode Mismatch.";

                    case 31:
                        return "Expl Number.";
                }
            }
            else
            {
                switch (errorCode)
                {
                    case 2:
                        return "Exercise.";

                    case 3:
                        return "Position liquidation.";

                    case 20:
                    case 5:
                        return "Expl Security.";

                    case 6:
                        return "Broker.";

                    case 7:
                        return "Branch.";

                    case 8:
                        return "User.";

                    case 9:
                        return "Participant.";

                    case 10:
                        return "Counter Party.";

                    case 11:
                        return "Order Number.";

                    case 15:
                        return "Auction Number.";

                    case 16:
                        return "Order.";

                    case 17:
                        return "Price Freeze.";

                    case 18:
                        return "Quantity Freeze.";

                    case 29:
                        return "Invalid Expl.";

                    case 30:
                        return "Exercise Mode Mismatch.";

                    case 31:
                        return "Expl Number.";
                }
            }
            return string.Empty;
        }
    }
}
