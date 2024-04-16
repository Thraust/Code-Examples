import discord
import pyodbc
import aiohttp
import asyncio
import async_timeout
from discord.ext import tasks, commands
from .sql import SQL
from bs4 import BeautifulSoup
from datetime import date, datetime
#from PSO2 import urgentquest as uq

class Pso(commands.Cog):

    def __init__(self,bot):
        self.bot = bot
        
    # Events
    @commands.Cog.listener()
    async def on_ready(self):
        print('PSO2 Ready')
    
    def is_it_me(ctx):
        if ctx.author.id == <ID Number>: # 
            return ctx.author.id == <ID Number>
    
    # Special!
    async def get_page(self):
        async with aiohttp.ClientSession() as session:
            async with session.get('https://pso2.com/news/urgent-quests/urgentquestschedule0901', timeout=10) as resp:
                #print(resp.status)
                page = await resp.text()
                await session.close()
                return page

    # Commands    
    @commands.command()
    @commands.check(is_it_me)
    async def clearUQ(self,ctx):
        try:
            sqlcur = SQL.opencur(ctx)
            
            sqlcur.execute('delete from pso.pso_events')
            sqlcur.execute('delete from pso.uq_dates')
            sqlcur.execute('delete from pso.uq_schedule')
            
            sqlcur.commit()
            sqlcur.close()
            
            await ctx.send("UQ Tables cleared!")
        except Exception as e:
            print(e)
            sqlcur.close()
            raise
        finally:
            pass
        
    @commands.command()
    @commands.check(is_it_me)
    async def updateUQ(self,ctx):
        try:
            page = await self.get_page()
            soup = BeautifulSoup(page, 'html.parser')
            
            activeSections = soup.find(id='active-section')
            contentfr = activeSections.find(class_='content fr-view')
            tables = contentfr.find_all('table')
            statements = [None]
            
            for t in range(len(list(tables))):
                entries = tables[t].find_all('td')
                statement = "insert into [pso].[uq_dates]([day1],[day2],[day3],[day4],[day3x2],[day5],[day6],[day3x3],[day7]) values("
                count = 0
                row = 0
                identifier = ""
                idcount = 0
                
                if t in [0,2]:
                    for e in range(len(list(entries))):
                        if row == 0 and count == 1:
                            count = 0
                            row += 1
                        elif (row == 1 or row == 2) and count > 6:
                            count = 0
                            row += 1
                            statement += "')"
                            
                            if "uq_dates" in statement:
                                statement = statement.replace("'WEEK 1',","")
                                statement = statement.replace("'WEEK 2',","")
                                statement = statement.replace("WEEK 1 | ","")
                                statement = statement.replace("WEEK 2 | ","")
                                statement = statement.replace("'","','")
                                statement = statement.replace(",','","")
                                statement = statement.replace("(',","(")
                                statement = statement.replace(",')",")")
                            
                            if "into(" not in statement:
                                #print(statement)
                                #statements += statement + "\n"
                                statements.append(statement)
                            
                            statement = "insert into() values("
                        elif count > 8:
                            count = 0
                            row += 1
                            statement += "')"
                            if "into(" not in statement:
                                #print(statement)
                                #statements += statement + "\n"
                                statements.append(statement)
                            statement = "insert into [pso].[uq_schedule]([identifier],[gmt],[pdt],[day1],[day2],[day3],[day4],[day5],[day6],[day7]) values("
                        
                        if len(entries[e].text) > 2:
                            # Get date/day column records
                            if "WEEK" not in entries[e].text:
                                if idcount == 0:
                                    statement += f"'{identifier}','" + entries[e].text
                                    statement = statement.replace("M","M',")
                                    idcount += 1
                                else:
                                    statement += "'" + entries[e].text
                                    idcount = 0
                                
                                if count == 2 and row == 1:
                                    identifier += " | " +entries[e].text
                                    #print(identifier)
                            # Get Week number
                            else:
                                identifier = entries[e].text
                        elif entries[e].prettify().find('background') != -1:
                            background = entries[e].prettify().find('background') + 11
                            start = entries[e].prettify()[background:].find('#') + background
                            end = start + 7
                            if t in [0,2]:
                                statement += "','" + entries[e].prettify()[start:end]
                        else:
                            statement += "','white"
                            
                        count += 1
                else:
                    color = ""
                    event = ""
                    count = 0
                    for e in range(len(list(entries))):
                        
                        if entries[e].prettify().find('background') != -1:
                            start = entries[e].prettify().find('background') + 12
                            end = entries[e].prettify()[start:].find(');padd')
                            
                            # Get numerical values of rgb string
                            rgb = entries[e].prettify()[start + 4:start + end]
                            rgb = rgb.replace(" ,",",")
                            
                            # Convert strip values of rgb
                            firstcomma = rgb.find(",")
                            secondcomma = firstcomma + 1 + rgb[firstcomma + 1:].find(",")
                            r = rgb[:firstcomma]
                            g = rgb[firstcomma + 2: secondcomma]
                            b = rgb[secondcomma + 2:]
                            #print(rgb)
                            #print(f"r={r},g={g},b={b}")
                            
                            if r != "":
                                rgbhex = '#%02x%02x%02x' % (int(r),int(g),int(b))
                                color = rgbhex
                                #print(rgbhex)
                            else:
                                color = 'black'
                                #print("black")
                        else:
                            event = entries[e].text
                            #print(event)
                        
                        if count == 0:
                            count += 1
                        else:
                            count = 0
                            statement = f"insert into [pso].[pso_events]([event_name],[color]) values('{event}','{color}')"
                            statements.append(statement)
                            #print(statements)
                            
            #print(statements)
            sqlcur = SQL.opencur(ctx)
            
            skipfirst = 0
            for s in list(statements):
                if skipfirst == 0 :
                    skipfirst = 1
                else:
                    #print(s)
                    sqlcur.execute(s)

            #row = sqlcur.fetchall()
            sqlcur.commit()
            sqlcur.close()
            await ctx.send("UQ Events Inserted!")
            
        except Exception as e:
            print(e)
            sqlcur.close()
            raise
        finally:
            pass
    
    @commands.command(aliases=['uq'])
    async def nextUQ(self,ctx,tz='pst'):
        try:
            # break down date and time to usable formats
            month = int(date.today().strftime("%m"))
            day = int(date.today().strftime("%d"))
            today = "{:2d}/{:2d}".format(month,day)
            today = today.replace(" ","")
            time =  datetime.now().strftime("%I:%M %p")
            
            # Stored procedure used to return data from .updateUQ web scrape
            statement = f"exec uq_date_schedule_sp '{today}', '{time}'"
            
            sqlcur = SQL.opencur(ctx)
            sqlcur.execute(statement)
            row = sqlcur.fetchall()
            
            # When there are no more UQ's in the day, roll to the next day
            if not row:
                day += 1
                today = "{:2d}/{:2d}".format(month,day)
                today = today.replace(" ","")
                time = '0:00 AM'
                statement = f"exec uq_date_schedule_sp '{today}', '{time}'"
                sqlcur.execute(statement)
                row = sqlcur.fetchall()
            
            sqlcur.commit()
            sqlcur.close()

            miliTime = row[0].PDT
            hours, minutes = miliTime.split(":")
            hours, minutes = int(hours), int(minutes)
            
            if tz == 'mst':
                hours += 1
            elif tz == 'cst':
                hours += 2
            elif tz == 'est':
                hours += 3
            
            setting = " AM"
            if hours > 12:
                setting = " PM"
                hours -= 12
            
            eventTime = ("%02d:%02d" + setting) % (hours, minutes)
            
            embed = discord.Embed(color=ctx.author.color)
            embed.set_author(name="Urgent Quest")
            embed.add_field(name=f"Next UQ is at {eventTime}",value=f"```{row[0].event_name}```", inline=True)
            await ctx.send(embed=embed)

        except Exception as e:
            print(e)
            sqlcur.close()
            raise
        finally:
            pass
    
    @updateUQ.error
    async def updateUQ_error(self, ctx, error):
        print('pso2.py')
        print(error)
        await ctx.send(error)
    
    @nextUQ.error
    async def nextUQ_error(self, ctx, error):
        print('pso2.py')
        print(error)
        await ctx.send(error)
        
def setup(bot):
    bot.add_cog(Pso(bot))