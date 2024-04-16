import discord
import pyodbc
import random
from discord.ext import commands

class DND(commands.Cog):
    
    def __init__(self,bot):
        self.client = bot
        
    # Events
    @commands.Cog.listener()
    async def on_ready(self):
        print('dnd Ready')
    
    def opencur(self):
        global sqlcon,sqlcur
        sqlcon = pyodbc.connect("<Connection String>")
        sqlcur = sqlcon.cursor()
        return
    
    def values(self,id,serv):
        self.opencur()
        sqlcur.execute(f"""select str,dex,con,int,wis,cha from dnd_stats s
                       join dnd_pc c on s.uid=c.uid where 
                       c.dis_id = '{id}' and
                       c.server_id = '{serv}'""")
        global row
        row = sqlcur.fetchall()
        sqlcur.commit()
        sqlcur.close()
        return row
    
    def modifier(self,score):
        if score == 1:
            return '-5'
        elif 2 <= score <= 3:
            return '-4'
        elif 4 <= score <= 5:
            return '-3'
        elif 6 <= score <= 7:
            return '-2'
        elif 8 <= score <= 9:
            return '-1'
        elif 10 <= score <= 11:
            return '+0'
        elif 12 <= score <= 13:
            return '+1'
        elif 14 <= score <= 15:
            return '+2'
        elif 16 <= score <= 17:
            return '+3'
        elif 18 <= score <= 19:
            return '+4'
        elif 20 <= score <= 21:
            return '+5'
        else:
            return '+0'
    
    @commands.command()
    async def regdnd(self,ctx):
        user = ctx.author.id 
        server = ctx.guild.id 
        name = ctx.author.name
        
        self.opencur()
        sqlcur.execute(f"""insert into dnd_player(dis_name,dis_id,server_id)
                       values('{name}','{user}','{server}')""")
        sqlcur.commit()
        sqlcur.close()
        
        print(f'Registered: {name} for D&D')
        await ctx.send(f'Registered!')
    
    @commands.command()
    async def dndstats(self,ctx):
        self.values(ctx.author.id,ctx.guild.id)

        embed = discord.Embed(title=f"D&D Stats: {ctx.author.name}", color=ctx.author.color)
        embed.add_field(name=f"STR: {row[0].str}",value=f"{self.modifier(row[0].str)}",inline=True)
        embed.add_field(name=f"INT: {row[0].int}",value=f"{self.modifier(row[0].int)}",inline=True)
        embed.add_field(name=f"DEX: {row[0].dex}",value=f"{self.modifier(row[0].dex)}",inline=True)
        embed.add_field(name=f"WIS: {row[0].wis}",value=f"{self.modifier(row[0].wis)}",inline=True)
        embed.add_field(name=f"CON: {row[0].con}",value=f"{self.modifier(row[0].con)}",inline=True)        
        embed.add_field(name=f"CHA: {row[0].cha}",value=f"{self.modifier(row[0].cha)}",inline=True)
        
        await ctx.send(embed=embed)
    
    @commands.command()
    async def roll(self,ctx,ability):
        self.values(ctx.author.id,ctx.guild.id)
        if ability.lower() == 'str':
            mod = self.modifier(row[0].str)
        elif ability.lower() == 'dex':
            mod = self.modifier(row[0].dex)
        elif ability.lower() == 'con':
            mod = self.modifier(row[0].con)
        elif ability.lower() == 'int':
            mod = self.modifier(row[0].int)
        elif ability.lower() == 'wis':
            mod = self.modifier(row[0].wis)
        elif ability.lower() == 'cha':
            mod = self.modifier(row[0].cha)
            
        rolled = random.randint(1,20)
        mod = mod.replace('+','')
        mod = int(mod.replace('-',''))
        
        await ctx.send(f'{ctx.author.name} - {ability.lower()} roll: {rolled} + {mod} = {rolled + mod}')
    
    # Error Handling
    @regdnd.error
    async def on_error(event, args, kwargs):
        sqlcur.commit()
        sqlcur.close()
        print(event, args, kwargs)
    
    @dndstats.error 
    async def on_error(event, args, kwargs):
        print(event, args, kwargs)
        
    @roll.error 
    async def on_error(event, args, kwargs):
        print(event, args, kwargs)
        
def setup(bot):
    bot.add_cog(DND(bot))